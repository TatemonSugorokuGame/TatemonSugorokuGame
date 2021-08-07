using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using System.Linq;
using Cysharp.Threading.Tasks;
using System.Threading;
//using TatemonSugoroku.Gfshade.Extension;


namespace TatemonSugoroku.Scripts
{
    public class UICanvas : MonoBehaviour
    {
        [SerializeField]
        TMPro.TextMeshProUGUI _PlayerName;
        public TMPro.TextMeshProUGUI PlayerName => _PlayerName;

        [SerializeField]
        WalkRemainingPanel _WalkRemaining;
        public WalkRemainingPanel WalkRemaining => _WalkRemaining;

        [SerializeField]
        PlayerStatusPanel[] _PlayerStatusPanels;
        public PlayerStatusPanel[] PlayerStatusPanels => _PlayerStatusPanels;


        private readonly Color WhiteTransparent = new Color(1f, 1f, 1f, 0f);
        private readonly Color White = new Color(1f, 1f, 1f, 1f);

        private Sequence changeWalkSeq = null;


        public void Initalize(IList<string> playerNames)
        {
            if (playerNames.Count != PlayerStatusPanels.Length)
            {
                throw new System.ArgumentException();
            }

            for (int i = 0; i < PlayerStatusPanels.Length; i++)
            {
                SetString(PlayerStatusPanels[i].Name, playerNames[i]);
            }
        }

        private void SetString(TMPro.TextMeshProUGUI tm, string str)
        {
            tm.SetText(str);
        }
        private void SetInt(TMPro.TextMeshProUGUI tm, int num)
        {
            tm.SetText("{0}", num);
        }

        public void SetCurrentPlayerName(string name)
        {
            PlayerName.SetText(name);
        }

        public void SetName(int playerId, string str)
        {
            SetString(PlayerStatusPanels[playerId].Name, str);
        }

        public void SetScore(int playerId, int num)
        {
            SetInt(PlayerStatusPanels[playerId].Score, num);
        }

        public void SetTatemon(int playerId, int num)
        {
            SetInt(PlayerStatusPanels[playerId].Tatemon, num);
        }

        public void SetWalkRemaining(int num)
        {
            _WalkRemaining.Current.gameObject.SetActive(true);
            _WalkRemaining.Reserved.gameObject.SetActive(false);
            SetInt(_WalkRemaining.Current, num);
        }

        public void HideWalkRemaining()
        {
            _WalkRemaining.Current.gameObject.SetActive(false);
            _WalkRemaining.Reserved.gameObject.SetActive(false);
        }


        public async UniTask ChangeScore(TMPro.TextMeshProUGUI tm, int oldScore, int newScore, float duration, CancellationToken ct = default)
        {
            await DOVirtual.Float(oldScore, newScore, duration, num => tm.SetText("{0}", (int)num)).Play().ToUniTask(TweenCancelBehaviour.Complete, ct);
        }
        public UniTask ChangeScore(int playerId, int oldScore, int newScore, float duration = 0.5f, CancellationToken ct = default)
        {
            return ChangeScore(PlayerStatusPanels[playerId].Score, oldScore, newScore, duration, ct);
        }

        public async UniTask ChangeTatemon(TMPro.TextMeshProUGUI tm, int oldTatemon, int newTatemon, float duration, CancellationToken ct = default)
        {
            Sequence seq = DOTween.Sequence()
                .Append(tm.transform.DOLocalMoveY(20f, duration).SetEase(Ease.OutSine).SetRelative())
                .AppendCallback(() => tm.SetText("{0}", newTatemon))
                .Append(tm.transform.DOLocalMoveY(-20f, duration).SetEase(Ease.InSine).SetRelative());
            await seq.Play().ToUniTask(TweenCancelBehaviour.Complete, ct);
        }
        public UniTask ChangeTatemon(int playerId, int oldTatemon, int newTatemon, float duration = 0.1f, CancellationToken ct = default)
        {
            return ChangeTatemon(PlayerStatusPanels[playerId].Tatemon, oldTatemon, newTatemon, duration, ct);
        }

        public async UniTask ChangeWalkRemaining(WalkRemainingPanel panel, int oldWalk, int newWalk, float duration, CancellationToken ct = default)
        {
            float fadeDuration = duration * 0.6f;
            float latterStart = duration * 0.4f;

            panel.Reserved.SetText("{0}", newWalk);
            panel.Reserved.gameObject.SetActive(true);
            panel.Reserved.color = WhiteTransparent;

            try
            {
                Gfshade.Utilities.TweenKill(changeWalkSeq, true);
                changeWalkSeq = DOTween.Sequence()
                    .Insert(0f, panel.Current.DOColor(WhiteTransparent, fadeDuration).SetEase(Ease.Linear))
                    .Insert(latterStart, panel.Reserved.DOColor(White, fadeDuration).SetEase(Ease.Linear));
                await changeWalkSeq.Play().ToUniTask(TweenCancelBehaviour.Complete, ct);
            }
            finally
            {
                panel.Current.gameObject.SetActive(false);
                panel.Swap();
            }
        }
        public UniTask ChangeWalkRemaining(int oldWalk, int newWalk, float duration = 0.5f, CancellationToken ct = default)
        {
            return ChangeWalkRemaining(WalkRemaining, oldWalk, newWalk, duration, ct);
        }
    }
}