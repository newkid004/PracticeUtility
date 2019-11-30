using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

public static class CustomRoutine
{
	private static CustomRoutineManager _Routiner = null;
	private static CustomRoutineManager Routiner
	{
		get
		{
			if (_Routiner == null)
			{
				GameObject goRoutiner = new GameObject("CustomRoutiner");
				GameObject.DontDestroyOnLoad(goRoutiner);

				_Routiner = goRoutiner.AddComponent<CustomRoutineManager>();
				_Routiner.Init();
			}
			return _Routiner;
		}
	}

	private class CustomRoutineManager : MonoBehaviour, IDisposable
	{
		private Dictionary<int, int> _dictCoroutineIndex;
		private Dictionary<int, Dictionary<int, Coroutine>> _dictManagementCoroutine;
		private int _currentSceneIndex = -1;

		private int NextCoroutineIndex { get => ++_dictCoroutineIndex[_currentSceneIndex]; }
		private Dictionary<int, Coroutine> CurrentCoroutineDict { get => _dictManagementCoroutine[_currentSceneIndex]; }

		public void Init()
		{
			_dictCoroutineIndex = new Dictionary<int, int>();
			_dictManagementCoroutine = new Dictionary<int, Dictionary<int, Coroutine>>();
			_currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

			SceneManager.activeSceneChanged += OnLevelWasLoaded;
		}

		public void Dispose()
		{
			SceneManager.activeSceneChanged -= OnLevelWasLoaded;
		}

		private void OnLevelWasLoaded(Scene sceneBefore, Scene sceneAfter)
		{
			StopSceneRoutine(sceneBefore.buildIndex);
			_currentSceneIndex = sceneAfter.buildIndex;
		}

		private void StopSceneRoutine(int index = -1)
		{
			if (index < 0)
				index = _currentSceneIndex;

			foreach (var sceneRoutine in _dictManagementCoroutine[index])
				StopCoroutine(sceneRoutine.Value);
		}

		public void StopRoutine(int index)
		{
			CurrentCoroutineDict.Remove(index);
			StopCoroutine(CurrentCoroutineDict[index]);
		}

		public int CallLate(float delay, Action callback, bool isRealtime = false)
		{
			int routineIndex = NextCoroutineIndex;

			Coroutine c = StartCoroutine(DoCallLate(delay, callback, isRealtime, routineIndex));
			CurrentCoroutineDict.Add(routineIndex, c);

			return routineIndex;
		}

		private IEnumerator DoCallLate(float delay, Action callback, bool isRealtime , int routineIndex)
		{
			int nowCurrentScene = _currentSceneIndex;

			if (isRealtime)
				yield return new WaitForSecondsRealtime(delay);
			else
				yield return new WaitForSeconds(delay);

			if (nowCurrentScene == _currentSceneIndex)
			{
				callback();
				CurrentCoroutineDict.Remove(routineIndex);
			}
		}

		public int CallLoop(Func<bool> callback, Action OnEnd = null)
		{
			int routineIndex = NextCoroutineIndex;

			Coroutine c = StartCoroutine(DoCallLoop(callback, OnEnd, routineIndex));
			CurrentCoroutineDict.Add(routineIndex, c);

			return routineIndex;
		}

		private IEnumerator DoCallLoop(Func<bool> callback, Action OnEnd, int routineIndex)
		{
			int nowCurrentScene = _currentSceneIndex;

			while (true)
			{
				if (nowCurrentScene != _currentSceneIndex || !callback())
					break;

				yield return null;
			}

			if (nowCurrentScene == _currentSceneIndex)
			{
				OnEnd?.Invoke();
				CurrentCoroutineDict.Remove(routineIndex);
			}
		}
	}

	public static void Stop(int routineIndex) => Routiner.StopRoutine(routineIndex);
	public static int CallLate(float delay, Action callback, bool isRealtime = false) => Routiner.CallLate(delay, callback, isRealtime);
	public static int CallLoop(Func<bool> callback, Action OnEnd = null) => Routiner.CallLoop(callback, OnEnd);

	public static int Pause(float Second, bool isRealtime = true)
	{
		float prevTime = Time.timeScale;
		Time.timeScale = 0.0f;

		return Routiner.CallLate(Second, () => Time.timeScale = prevTime, isRealtime);
	}
} //
