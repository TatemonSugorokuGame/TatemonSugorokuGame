using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System.Threading;
using SubmarineMirage.Service;
using SubmarineMirage.Audio;
using SubmarineMirage.Scene;
using SubmarineMirage.Task;
using SubmarineMirage.Setting;

namespace TatemonSugoroku.Scripts
{
    public class ResultCanvas : MonoBehaviour
    {
        [SerializeField]
        SwitchableImage _Winner;

        [SerializeField]
        ResultScorePanel[] _ResultScores;

        [SerializeField]
        Button _Button;


        public async UniTask WaitForClick(IList<int> scores, int winner, CancellationToken ct)
        {
            var audioManager = SMServiceLocator.Resolve<SMAudioManager>();
            await UniTask.WhenAll(
                audioManager.Stop<SMBGM>(),
                audioManager.Stop<SMBGS>()
            );
            var uiGameEnd = FindObjectOfType<UIGameEnd>();
            uiGameEnd.SetActive( false );

            gameObject.SetActive(true);
            _Winner.Switch(winner);
            for (int i = 0; i < scores.Count; i++)
            {
                _ResultScores[i].Image.Switch(i);
                _ResultScores[i].Score.SetText("{0}", scores[i]);
            }

            audioManager.Play( SMBGM.Result ).Forget();
            audioManager.Play( SMBGS.Night ).Forget();
            await audioManager.Play( SMJingle.Result1 );
            await audioManager.Play( SMJingle.Result2 );

            await _Button.OnClickAsync(ct);

            audioManager.Play( SMSE.Result ).Forget();
            await UniTask.Delay( 500, cancellationToken: ct);

            var sceneManager = await SMServiceLocator.WaitResolve<SMSceneManager>();
            sceneManager.GetFSM<MainSMScene>().ChangeState<NetworkSMScene>().Forget();

            gameObject.SetActive( false );
        }
    }
}
