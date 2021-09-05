using System.Collections.Generic;
using System.Linq;
using SubmarineMirage.Base;
using SubmarineMirage.Debug;
using SubmarineMirage.Service;
using SubmarineMirage.Audio;
using SubmarineMirage.Setting;
using UniRx;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace TatemonSugoroku.Scripts.Akio
{
    class PlayerInternalModel
    {
        public int Id;
        public string Name;
        public int Score;
        public int OppositeEnterBonus;
        public int Tatemon;
        public int MaxTatemon;
        public int TileId;
    }

    static partial class Utility
    {
        public static int MaxIndex<T>(IList<T> list)
            where T : System.IComparable<T>
        {
            if (list.Count == 0) return -1;

            int max_index = 0;
            T max_element = list[0];
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].CompareTo(max_element) > 0)
                {
                    max_index = i;
                    max_element = list[i];
                }
            }
            return max_index;
        }
    }

    // Gfshade: 大改編
    public class MainGameManager : MonoBehaviour
    {
        [SerializeField]
        private UICanvas _UI;

        [SerializeField]
        private UITurn _TurnUI;

        [SerializeField]
        private DiceCanvas _DiceUI;

        [SerializeField]
        private UIFireworkView _FireworkUI;

        [SerializeField]
        private UIGameEnd _GameEndUI;

        [SerializeField]
        private ResultCanvas _ResultUI;

        [SerializeField]
        private DiceManagerView _DiceManager;

        [SerializeField]
        private TileManagerView _TileManager;

        [SerializeField]
        private PieceManagerView _PieceManager;

        [SerializeField]
        private MoveArrowManagerView _MoveArrowManager;

        [SerializeField]
        private TatemonManagerView _TatemonManager;

        [SerializeField]
        private DayView _DayManager;

        [SerializeField]
        private GameCameraView _CameraManager;


        private readonly int[] _SpinPowersOfTatemon = { 0, 0, 6, 5, 5, 4, 4, 3, 3, 2, 2, 1, 1 };
        private readonly int _OppositeEnterBonusOne = 20;
        private readonly int _MaxTurn = 7;

        private FieldModel fieldModel;
        private MotionModel motionModel;
        SMInputManager inputManager;
        PlayerInternalModel[] playerModels;
        SMAudioManager audioManager;



        private void InitTestPlayerInternalModels()
        {
            playerModels = new PlayerInternalModel[]
            {
                new PlayerInternalModel
                {
                    Id = 0,
                    Name = "たてた",
                    Score = 0,
                    OppositeEnterBonus = 0,
                    Tatemon = _MaxTurn,
                    MaxTatemon = _MaxTurn,
                    TileId = 0,
                },
                new PlayerInternalModel
                {
                    Id = 1,
                    Name = "たてて",
                    Score = 0,
                    OppositeEnterBonus = 0,
                    Tatemon = _MaxTurn,
                    MaxTatemon = _MaxTurn,
                    TileId = 63,
                },
            };
        }

        public async UniTask DoGame(CancellationToken ct)
        {
            InitTestPlayerInternalModels();
            inputManager = SMServiceLocator.Resolve<SMInputManager>();

            fieldModel = new FieldModel();
            fieldModel.InitializeGame(playerModels.Length);

            motionModel = new MotionModel();
            fieldModel.InitializeGame(playerModels.Length);

            for (int i = 0; i < 2; i++)
            {
                _TileManager.ChangeArea(playerModels[i].TileId, i);
            }

            InitUI();
            System.TimeSpan wait02 = System.TimeSpan.FromSeconds(0.2);
            System.TimeSpan wait10 = System.TimeSpan.FromSeconds(1.0);

            // げーむがはじまるよ
            await GameStart(ct);

            for (int i = 0; i < _MaxTurn; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    bool turnResult = await DoPlayerTurn(i, j, ct);
                    if (!turnResult) goto GameEnd;

                    // たてもんが置かれるたび、それのスコアは再計算される。
                    await UpdateScore(ct);
                }

                // じかんがすすむよ
                _DayManager.UpdateHour();

                // スコア更新時の時間調整だよ
                await UniTask.Delay(wait10, cancellationToken: ct);

                await UniTask.WaitWhile( () => _FireworkUI._isActive, cancellationToken: ct );
            }

            /// 3歩あるいたあとはスコア更新しなくてもいい気がするかなあ

            GameEnd:
            // げーむがおわったよ
            await GameEnd(ct);

            // 「そして、オレが勝者となる」
            List<int> scores = playerModels.Select(p => p.Score).ToList();
            int winnerID = 2;
            if      ( scores[0] > scores[1] )   { winnerID = 0; }
            else if ( scores[0] < scores[1] )   { winnerID = 1; }
            else                                { winnerID = 2; }
            await _ResultUI.WaitForClick(scores, winnerID, ct);
        }

        private async UniTask<bool> DoPlayerTurn(int turnIndex, int playerId, CancellationToken ct)
        {
            System.TimeSpan wait02 = System.TimeSpan.FromSeconds(0.2);
            System.TimeSpan wait10 = System.TimeSpan.FromSeconds(1.0);

            PlayerInternalModel pTurn = playerModels[playerId];

            // 「たておのターン！」
            await TurnStart(pTurn, ct);

            // 「ドロー！」
            int dice = await DoDice(playerId, ct);

            // 「オレは、12を召喚！」
            _UI.SetWalkRemaining(dice);
            await UniTask.Delay(wait02, cancellationToken: ct);

            // 「フィールドを、好きな方向に12回まで塗りつぶすことができるッ！」
            await InputAndMove(playerId, dice, ct);

            await UniTask.Delay(wait02, cancellationToken: ct);

            // 「さらに！　塗りつぶしが終了したとき、効果発動！オレの最終位置に、設置魔法「たてもん」が発動するぜ！」
            int spinPower = _SpinPowersOfTatemon[dice];
            if ( _DayManager._state.Value == DayState.Night )
            {
                // 「最終ターンである場合、さらに効果発動！「たてもん」のパワーが、2倍になるぜッ！」
                spinPower *= 2;
            }

            // 「モンスター「たてもん」が召喚される！」
            fieldModel.PutTatemonAtCurrentPosition(playerId, spinPower);
            _TatemonManager.Place(playerId, pTurn.TileId, spinPower);
            await UniTask.Delay(wait02, cancellationToken: ct);
            await _UI.ChangeTatemon(playerId, pTurn.Tatemon, pTurn.Tatemon - 1, 0.1f, ct);
            playerModels[playerId].Tatemon -= 1;

            // ターン終了時の時間調整だよ
            await UniTask.Delay(wait10, cancellationToken: ct);

            return true;
        }

        private void InitUI()
        {
            for (int i = 0; i < playerModels.Length; i++)
            {
                _UI.SetScore(i, playerModels[i].Score);
                _UI.SetTatemon(i, playerModels[i].Tatemon);
                _UI.SetMaxTatemon(i, playerModels[i].MaxTatemon);
            }
        }

        private async UniTask GameStart(CancellationToken ct)
        {
            SMLog.Debug($"ゲーム開始", SMLogTag.Scene);
            //await UniTask.Delay(System.TimeSpan.FromSeconds(5), cancellationToken: ct);

            audioManager = await SMServiceLocator.WaitResolve<SMAudioManager>();
            await audioManager.Play( SMJingle.Start1 );
            await audioManager.Play( SMJingle.Start2 );
        }

        /// <summary>
        /// ！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！
        /// ！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！
        /// ！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！
        /// ！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！
        /// ゲーム終了時に演出をするぞ！！！！！！！！！！！！！！！！！！！！！！
        /// ！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！
        /// ！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！
        /// ！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！
        /// ！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！
        /// </summary>
        private async UniTask GameEnd(CancellationToken ct)
        {
            SMLog.Debug($"ゲーム終了", SMLogTag.Scene);
            _UI.gameObject.SetActive(false);
            _TurnUI.ChangeTurn( null );
            _GameEndUI.SetActive( true );
            _CameraManager.SetResultCamera();
            await UniTask.Delay(System.TimeSpan.FromSeconds(1), cancellationToken: ct);
        }

        private async UniTask TurnStart(PlayerInternalModel model, CancellationToken ct)
        {
            SMLog.Debug($"{model.Name}のターン", SMLogTag.Scene);
            //_UI.SetCurrentPlayerName(model.Name);
            await UniTask.Delay(System.TimeSpan.FromSeconds(1), cancellationToken: ct);
            _TurnUI.ChangeTurn( model.Id );
        }

        private async UniTask<int> DoDice(int playerId, CancellationToken ct)
        {
            SMLog.Debug($"サイコロボタンを押してください", SMLogTag.Scene);
            await _DiceManager.ChangeState( DiceState.Rotate );
            await _DiceUI.WaitForClick(playerId, ct);
            int dice = await _DiceManager.Roll();
            
            SMLog.Debug($"{dice}が出ました", SMLogTag.Scene);
            return dice;
        }

        private async UniTask<bool> InputAndMove(int playerId, int dice, CancellationToken ct)
        {
            PlayerInternalModel pTurn = playerModels[playerId];

            // MotionModelを初期化する
            if (!ResetMotionModel(playerId, dice))
            {
                // 初期化時、囲まれてて移動できないならfalseが返る。
                // そうなったらゲーム終了
                return false;
            }

            Queue<int> motions;

            // 例外が飛んだ場合は購読していたという事実は無かったことになる
            using (CompositeDisposable movementDisposables = new CompositeDisposable())
            {
                // 移動ルート受け取り用のSubjectを用意
                Subject<Queue<int>> motionQueueSubject = new Subject<Queue<int>>();

                // 触った場所を購読して、MotionModelに流す
                // 最終結果は移動ルート受け取り用Subjectに向かってぷっしゅしてもらう
                inputManager
                    ._touchTileID
                    .Subscribe(tileId => InputPosition(motionQueueSubject, motionModel, tileId))
                    .AddTo(movementDisposables);

                // コマ移動
                /*
                fieldModel
                    .PlayerPositions
                    .Select(pp => pp[playerId])
                    .Subscribe(tileId => GoastPieceMove(playerId, tileId))
                    .AddTo(movementDisposables);
                */

                _PieceManager.PlaceDummy( playerId );

                // まず(矢印方向, 矢印状態)のぺあにおいて変化が全部送られてくるストリームをつくる
                var arrowUp = motionModel.MotionStatusUp.Select(status => new KeyValuePair<MoveArrowType, MotionStatus>(MoveArrowType.Up, status));
                var arrowRight = motionModel.MotionStatusRight.Select(status => new KeyValuePair<MoveArrowType, MotionStatus>(MoveArrowType.Right, status));
                var arrowDown = motionModel.MotionStatusDown.Select(status => new KeyValuePair<MoveArrowType, MotionStatus>(MoveArrowType.Down, status));
                var arrowLeft = motionModel.MotionStatusLeft.Select(status => new KeyValuePair<MoveArrowType, MotionStatus>(MoveArrowType.Left, status));
                var arrow = Observable.CombineLatest(arrowUp, arrowRight, arrowDown, arrowLeft);

                // 現在の矢印取得
                arrow
                    .First()
                    .Subscribe(arrowInfos => _MoveArrowManager.Place(playerId, pTurn.TileId, arrowInfos));

                // コマ移動が起こったら、
                // コマ座標と最新矢印情報をまとめて矢印表示に流す
                motionModel
                    .PeekPosition
                    .WithLatestFrom(arrow, (peek, arrow) => (peek, arrow))
                    .Subscribe(p => _MoveArrowManager.Place(playerId, p.peek, p.arrow))
                    .AddTo(movementDisposables);

                // 残り歩数をUIに流す
                motionModel
                    .NumberOfMovableCells
                    .Subscribe(_UI.SetWalkRemaining)
                    .AddTo(movementDisposables);


                // 移動終了まで待つ
                motions = await motionQueueSubject.ToUniTask(cancellationToken: ct);
                pTurn.TileId = motions.Last();
                HideArrows();
            }
            await _PieceManager.WaitMoves();

            // ルートを塗る
            while (motions.Count > 0)
            {
                int tileId = motions.Dequeue();
                var moveResult = fieldModel.MovePlayer(playerId, tileId);

                // 「罠カードオープン！　おまえがオレの領域(テリトリー)を踏んだ時、オレは20点のスコアを得る！」
                if (moveResult.IsOppositeEnter)
                {
                    playerModels[moveResult.oppositePlayerId].OppositeEnterBonus += _OppositeEnterBonusOne;
                }

                // 塗る
                _TileManager.ChangeArea(tileId, playerId);

                await UniTask.Delay( 50, cancellationToken: ct );
            }

            return true;
        }

        private void HideArrows()
        {
            _MoveArrowManager.Hide();
            _PieceManager.HideDummies();
            _UI.HideWalkRemaining();
        }

        private async UniTask UpdateScore(CancellationToken ct)
        {
            List<UniTask> changeScore = new List<UniTask>();
            int[] newScore = new int[playerModels.Length];

            for (int i = 0; i < 2; i++)
            {
                newScore[i] = ScoreModel.CalculateFieldScore(fieldModel.GetFieldCells(), i) + playerModels[i].OppositeEnterBonus;
                changeScore.Add(_UI.ChangeScore(i, playerModels[i].Score, newScore[i], 0.5f, ct));
            }

            await UniTask.WhenAll(changeScore);
            
            for (int i = 0; i < 2; i++)
            {
                playerModels[i].Score = newScore[i];
            }
        }


        private void InputPosition(System.IObserver<Queue<int>> onMoveComplete, MotionModel model, int tileId)
        {
            model.PushMotionPosition(tileId);
            if (model.InspectInputtingMotionsFinished())
            {
                var motionQueue = model.GetMotionsAsQueue();
                model.ClearMotion();
                onMoveComplete.OnNext(motionQueue);
                onMoveComplete.OnCompleted();
            }
        }


        private bool ResetMotionModel(int currentPlayerId, int numberOfDice)
        {
            motionModel.SetCurrentPlayerId(currentPlayerId);
            motionModel.SetNumberOfDice(numberOfDice);
            motionModel.SetCurrentPosition(fieldModel.GetCurrentPositionByPlayerId(currentPlayerId));
            motionModel.SetFieldCellsAsMovableField(fieldModel.GetFieldCells());
            motionModel.ClearInformation();

            return motionModel.InspectPlayerCanMove();
        }

    }
}
