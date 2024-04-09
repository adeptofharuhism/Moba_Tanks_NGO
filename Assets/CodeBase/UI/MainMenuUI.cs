using Assets.CodeBase.Infrastructure.Services;
using Assets.CodeBase.Infrastructure.Services.Connection;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.CodeBase.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private UIDocument _uiDocument;
        [SerializeField] private VisualTreeAsset _hostSettingsPanel;
        [SerializeField] private VisualTreeAsset _joinSettingsPanel;

        private VisualElement _settingsRoot;

        private VisualElement _hostSettingsPanelInstantiated;
        private TextField _hostPlayerNameInput;
        private TextField _hostIPInput;
        private TextField _hostPortInput;

        private VisualElement _joinSettingsPanelInstantiated;
        private TextField _joinPlayerNameInput;
        private TextField _joinIPInput;
        private TextField _joinPortInput;

        private IConnectionService _connectionService;

        private void Awake() => 
            _connectionService = AllServices.Container.Single<IConnectionService>();

        private void OnEnable() {
            SetupPanels();
            SetupMenuButtons();
            SetupHostSettingsButtons();
            SetupJoinLobbySettingsButtons();
        }

        private void SetupPanels() {
            _settingsRoot = _uiDocument.rootVisualElement.Q<VisualElement>(Constants.VisualElementNames.MainMenu.SettingsContainer);

            _hostSettingsPanelInstantiated = _hostSettingsPanel.Instantiate();
            _hostSettingsPanelInstantiated.StretchToParentSize();

            _joinSettingsPanelInstantiated = _joinSettingsPanel.Instantiate();
            _joinSettingsPanelInstantiated.StretchToParentSize();
        }

        private void SetupMenuButtons() {
            VisualElement root = _uiDocument.rootVisualElement;

            root.Q<Button>(Constants.VisualElementNames.MainMenu.MenuPanel.HostButton)
                .RegisterCallback<ClickEvent>(OnClickHostLobbyButton);
            root.Q<Button>(Constants.VisualElementNames.MainMenu.MenuPanel.JoinButton)
                .RegisterCallback<ClickEvent>(OnClickJoinLobbyButton);
            root.Q<Button>(Constants.VisualElementNames.MainMenu.MenuPanel.ExitButton)
                .RegisterCallback<ClickEvent>(OnClickExitButton);
        }

        private void SetupHostSettingsButtons() {
            _hostPlayerNameInput = _hostSettingsPanelInstantiated.Q<TextField>(Constants.VisualElementNames.MainMenu.PlayerNameInput);
            _hostIPInput = _hostSettingsPanelInstantiated.Q<TextField>(Constants.VisualElementNames.MainMenu.IPInput);
            _hostPortInput = _hostSettingsPanelInstantiated.Q<TextField>(Constants.VisualElementNames.MainMenu.PortInput);

            _hostSettingsPanelInstantiated.Q<Button>(Constants.VisualElementNames.MainMenu.HostPanel.HostButton)
                .RegisterCallback<ClickEvent>(OnClickHostPanelHostButton);
            _hostSettingsPanelInstantiated.Q<Button>(Constants.VisualElementNames.MainMenu.CancelButton)
                .RegisterCallback<ClickEvent>(OnClickCancelButton);
        }

        private void SetupJoinLobbySettingsButtons() {
            _joinPlayerNameInput = _joinSettingsPanelInstantiated.Q<TextField>(Constants.VisualElementNames.MainMenu.PlayerNameInput);
            _joinIPInput = _joinSettingsPanelInstantiated.Q<TextField>(Constants.VisualElementNames.MainMenu.IPInput);
            _joinPortInput= _joinSettingsPanelInstantiated.Q<TextField>(Constants.VisualElementNames.MainMenu.PortInput);

            _joinSettingsPanelInstantiated.Q<Button>(Constants.VisualElementNames.MainMenu.JoinPanel.JoinButton)
                .RegisterCallback<ClickEvent>(OnClickJoinPanelJoinButton);
            _joinSettingsPanelInstantiated.Q<Button>(Constants.VisualElementNames.MainMenu.CancelButton)
                .RegisterCallback<ClickEvent>(OnClickCancelButton);
        }

        private void OnClickHostLobbyButton(ClickEvent e) {
            ClearSettingsRoot();
            _settingsRoot.Add(_hostSettingsPanelInstantiated);
        }

        private void OnClickJoinLobbyButton(ClickEvent e) {
            ClearSettingsRoot();
            _settingsRoot.Add(_joinSettingsPanelInstantiated);
        }

        private void OnClickExitButton(ClickEvent e) =>
            Application.Quit();

        private void OnClickHostPanelHostButton(ClickEvent e) {
            _connectionService.StartHostIP(
                _hostPlayerNameInput.text, 
                _hostIPInput.text, 
                int.Parse(_hostPortInput.text));
        }

        private void OnClickJoinPanelJoinButton(ClickEvent e) {
            _connectionService.StartClientIP(
                _joinPlayerNameInput.text,
                _joinIPInput.text,
                int.Parse(_joinPortInput.text));
        }

        private void OnClickCancelButton(ClickEvent e) => 
            ClearSettingsRoot();

        private void ClearSettingsRoot() {
            if (_settingsRoot.childCount > 0)
                _settingsRoot.RemoveAt(0);
        }
    }
}
