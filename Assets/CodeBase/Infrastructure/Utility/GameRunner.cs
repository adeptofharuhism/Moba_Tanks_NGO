using UnityEngine;

namespace Assets.CodeBase.Infrastructure.Utility
{
    public class GameRunner : MonoBehaviour
    {
        [SerializeField] private GameBootstrapper _bootstrapperPrefab;

        private void Awake() {
            GameBootstrapper bootstrapper = FindFirstObjectByType<GameBootstrapper>();

            if (bootstrapper == null)
                Instantiate(_bootstrapperPrefab);

            Destroy(gameObject);
        }
    }
}
