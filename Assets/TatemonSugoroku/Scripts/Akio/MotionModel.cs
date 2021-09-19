using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace TatemonSugoroku.Scripts.Akio
{
    public enum MotionStatus
    {
        Unmovable,
        Movable,
        Return
    }
    
    public class Vertex2
    {
        public int V;
        public int BeforeV;
        public int Depth;
    }
    
    public class MotionModel: IModel
    {
        private const int MAX_NUMBER_OF_CELLS = 64;
        private const int MAX_X_DIRECTION_OF_CELLS = 8;
        private const int MAX_Y_DIRECTION_OF_CELLS = 8;
        
        private int _currentPlayerId;
        private int _numberOfDice;
        private int _currentPosition;

        private readonly bool[] _movableField = new bool[MAX_NUMBER_OF_CELLS];

        private readonly List<int> _listPreparedMotions = new List<int>();
        private readonly HashSet<int> _movablePositions = new HashSet<int>();
        
        private readonly Subject<int[]> _preparedMotions = new Subject<int[]>();
        public IObservable<int[]> PreparedMotions => _preparedMotions;

        private readonly ReactiveProperty<MotionStatus> _motionStatusUp =
            new ReactiveProperty<MotionStatus>(MotionStatus.Unmovable);

        private readonly ReactiveProperty<MotionStatus> _motionStatusRight =
            new ReactiveProperty<MotionStatus>(MotionStatus.Unmovable);
        
        private readonly ReactiveProperty<MotionStatus> _motionStatusDown =
            new ReactiveProperty<MotionStatus>(MotionStatus.Unmovable);

        private readonly ReactiveProperty<MotionStatus> _motionStatusLeft =
            new ReactiveProperty<MotionStatus>(MotionStatus.Unmovable);

        public IReadOnlyReactiveProperty<MotionStatus> MotionStatusUp => _motionStatusUp;
        public IReadOnlyReactiveProperty<MotionStatus> MotionStatusRight => _motionStatusRight;
        public IReadOnlyReactiveProperty<MotionStatus> MotionStatusDown => _motionStatusDown;
        public IReadOnlyReactiveProperty<MotionStatus> MotionStatusLeft => _motionStatusLeft;

        private readonly ReactiveProperty<int> _numberOfMovableCells = new ReactiveProperty<int>(0);
        public IReadOnlyReactiveProperty<int> NumberOfMovableCells => _numberOfMovableCells;

        private readonly ReactiveProperty<int> _peekPosition = new ReactiveProperty<int>(0);
        public IReadOnlyReactiveProperty<int> PeekPosition => _peekPosition;
        public MotionModel()
        {
            for (int i = 0; i < MAX_NUMBER_OF_CELLS; i++)
            {
                _movableField[i] = false;
            }
        }

        public void Initialize( AllModelManager manager ) {
        }
        
        public void InitializeGame()
        {
            for (int i = 0; i < MAX_NUMBER_OF_CELLS; i++)
            {
                _movableField[i] = false;
            }
        }
        
        public void SetCurrentPlayerId(int playerId)
        {
            _currentPlayerId = playerId;
        }
        public void SetNumberOfDice(int numberOfDice)
        {
            _numberOfDice = numberOfDice;
        }
        public void SetCurrentPosition(int position)
        {
            _currentPosition = position;
        }

        public void SetFieldCellsAsMovableField(FieldCell[] fieldCells)
        {
            for (int i = 0; i < MAX_NUMBER_OF_CELLS; i++)
            {
                _movableField[i] = fieldCells[i].TatemonPlayerId < 0 ||
                                   fieldCells[i].TatemonPlayerId == _currentPlayerId;
            }
        }
        
        public bool InspectPlayerCanMove()
        {
            List<List<int>> graph = new List<List<int>>();
            for (int i = 0; i < MAX_NUMBER_OF_CELLS; i++)
            {
                graph.Add(new List<int>());
            }

            for (int i = 0; i < MAX_NUMBER_OF_CELLS; i++)
            {
                if (!_movableField[i])
                {
                    continue;
                }

                if (i % MAX_X_DIRECTION_OF_CELLS < MAX_X_DIRECTION_OF_CELLS - 1) 
                {
                    int j = i + 1;
                    if (_movableField[j])
                    {
                        graph[i].Add(j);
                        graph[j].Add(i);
                    }
                }

                if (i / MAX_Y_DIRECTION_OF_CELLS < MAX_Y_DIRECTION_OF_CELLS - 1)
                {
                    int j = i + MAX_X_DIRECTION_OF_CELLS;
                    if (_movableField[j])
                    {
                        graph[i].Add(j);
                        graph[j].Add(i);
                    }
                }
            }

            HashSet<int> hashSetPositions = new HashSet<int>();
            
            Stack<Vertex2> stack = new Stack<Vertex2>();

            stack.Push(new Vertex2
            {
                V = _currentPosition,
                BeforeV = -1,
                Depth = 0
            });
            
            while (stack.Count > 0)
            {
                Vertex2 v = stack.Pop();

                if (v.Depth >= _numberOfDice)
                {
                    hashSetPositions.Add(v.V);
                    continue;
                }

                foreach (int nv in graph[v.V])
                {
                    if (nv == v.BeforeV)
                    {
                        continue;
                    }

                    stack.Push(new Vertex2
                    {
                        V = nv,
                        BeforeV = v.V,
                        Depth = v.Depth + 1
                    });
                }
            }

            return hashSetPositions.Count > 0;
        }
        public void PushMotionPosition(int position)
        {
            if (_listPreparedMotions.Count >= 2)
            {
                if (_listPreparedMotions[_listPreparedMotions.Count - 2] == position)
                {
                    _listPreparedMotions.RemoveAt(_listPreparedMotions.Count - 1);
                }
                else if (_movablePositions.Contains(position))
                {
                    _listPreparedMotions.Add(position);
                }
            }
            else if (_listPreparedMotions.Count == 1)
            {
                if (_currentPosition == position)
                {
                    _listPreparedMotions.RemoveAt(_listPreparedMotions.Count - 1);
                }
                else if (_movablePositions.Contains(position))
                {
                    _listPreparedMotions.Add(position);
                }
            }
            else if (_movablePositions.Contains(position))
            {
                _listPreparedMotions.Add(position);
            }
            
            UpdateMotionStatus();
        }

        public void ClearInformation()
        {
            _listPreparedMotions.Clear();
            UpdateMotionStatus();
        }

        void UpdateMotionStatus()
        {
            _movablePositions.Clear();

            _motionStatusUp.Value = MotionStatus.Unmovable;
            _motionStatusRight.Value = MotionStatus.Unmovable;
            _motionStatusDown.Value = MotionStatus.Unmovable;
            _motionStatusLeft.Value = MotionStatus.Unmovable;
            
            int previousPosition;
            int currentPosition;
            
            if (_listPreparedMotions.Count >= 2)
            {
                previousPosition = _listPreparedMotions[_listPreparedMotions.Count - 2];
                currentPosition = _listPreparedMotions[_listPreparedMotions.Count - 1];
            }
            else if (_listPreparedMotions.Count == 1)
            {
                previousPosition = _currentPosition;
                currentPosition = _listPreparedMotions[_listPreparedMotions.Count - 1];
            }
            else
            {
                previousPosition = -1;
                currentPosition = _currentPosition;
            }

//            Debug.Log(currentPosition);
            
            if (currentPosition >= MAX_X_DIRECTION_OF_CELLS)
            {
                int index = currentPosition - MAX_X_DIRECTION_OF_CELLS;

                if (previousPosition == index)
                {
                    _motionStatusUp.Value = MotionStatus.Return;
                }
                else if (_movableField[index])
                {
                    _movablePositions.Add(index);
                    _motionStatusUp.Value = MotionStatus.Movable;
                }
            }

            if (currentPosition % MAX_X_DIRECTION_OF_CELLS < MAX_X_DIRECTION_OF_CELLS - 1)
            {
                int index = currentPosition + 1;

                if (previousPosition == index)
                {
                    _motionStatusRight.Value = MotionStatus.Return;
                }
                else if (_movableField[index])
                {
                    _movablePositions.Add(index);
                    _motionStatusRight.Value = MotionStatus.Movable;
                }
            }

            if (currentPosition / MAX_X_DIRECTION_OF_CELLS < MAX_Y_DIRECTION_OF_CELLS - 1)
            {
                int index = currentPosition + MAX_X_DIRECTION_OF_CELLS;

                if (previousPosition == index)
                {
                    _motionStatusDown.Value = MotionStatus.Return;
                }
                else if (_movableField[index])
                {
                    _movablePositions.Add(index);
                    _motionStatusDown.Value = MotionStatus.Movable;
                }
            }

            if (currentPosition % MAX_X_DIRECTION_OF_CELLS > 0)
            {
                int index = currentPosition - 1;

                if (previousPosition == index)
                {
                    _motionStatusLeft.Value = MotionStatus.Return;
                }
                else if (_movableField[index])
                {
                    _movablePositions.Add(index);
                    _motionStatusLeft.Value = MotionStatus.Movable;
                }
            }

            _preparedMotions.OnNext(_listPreparedMotions.ToArray());

            _numberOfMovableCells.Value = _numberOfDice - _listPreparedMotions.Count;

            if (_listPreparedMotions.Count >= 1)
            {
                _peekPosition.Value = _listPreparedMotions[_listPreparedMotions.Count - 1];
            }
            else
            {
                _peekPosition.Value = _currentPosition;
            }
        }

        public bool InspectInputtingMotionsFinished()
        {
            return _listPreparedMotions.Count >= _numberOfDice;
        }

        public void ClearMotion()
        {
            _motionStatusUp.Value = MotionStatus.Unmovable;
            _motionStatusRight.Value = MotionStatus.Unmovable;
            _motionStatusDown.Value = MotionStatus.Unmovable;
            _motionStatusLeft.Value = MotionStatus.Unmovable;
        }

        public Queue<int> GetMotionsAsQueue()
        {
            Queue<int> r = new Queue<int>();
            foreach (int position in _listPreparedMotions)
            {
                r.Enqueue(position);
            }

            return r;
        }
        public void Dispose()
        {
            
        }
    }
}
