using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.CodeBase.Infrastructure
{
    public class SceneLoader
    {
        private readonly ICoroutineRunner _coroutineRunner;

        public SceneLoader(ICoroutineRunner coroutineRunner) => 
            _coroutineRunner = coroutineRunner;

        public void Load(string sceneName, Action onLoaded = null) =>
            _coroutineRunner.StartCoroutine(LoadScene(sceneName, onLoaded));

        public IEnumerator LoadScene(string nextScene, Action onLoaded) {
            AsyncOperation asyncLoadOperation = SceneManager.LoadSceneAsync(nextScene);

            while (!asyncLoadOperation.isDone)
                yield return null;

            onLoaded?.Invoke();
        }
    }
}
