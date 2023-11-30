using UnityEngine;
using UnityEngine.UI;

namespace Assets.CodeBase.UI.MainMenu
{
    public class LobbyPanel : MonoBehaviour
    {
        [SerializeField] private Button _cancelButton;

        private void Start() {
            LobbyPanelInitialize();
        }

        protected void LobbyPanelInitialize() {
            _cancelButton.onClick.AddListener(CancelClick);
        }

        protected void CancelClick() {
            gameObject.SetActive(false);
        }
    }
}
