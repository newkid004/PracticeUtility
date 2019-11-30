using System.Collections;
using System.Collections.Generic;

public static class CollectionExtend
{
#region // ----- List ----- //

	public static int CountActive(this List<GameObject> list)
	{
		for (int i = 0; i < list.Count; ++i)
		{
			if (!list[i].activeSelf)
				return i;
		}
		return list.Count;
	}

	public static int CountActive<T>(this List<T> list) where T : Component
	{
		for (int i = 0; i < list.Count; ++i)
		{
			if (!list[i].gameObject.activeSelf)
				return i;
		}
		return list.Count;
	}

	public static void Shuffle<T>(this List<T> list)
	{
		int nCount = list.Count;

		for (int i = 0; i < nCount; ++i)
		{
			int nIdxRandom = Random.Range(0, nCount);

			T temp = list[i];
			list[i] = list[nIdxRandom];
			list[nIdxRandom] = temp;
		}
	}

	public static void Shuffle<T>(this List<T> list, int nRangeMin = -1, int nRangeMax = -1)
	{
		int nCount = list.Count;
	
		for (int i = 0; i < nCount; ++i)
		{
			int nIdxRandom = Random.Range(0, nCount);
	
			T temp = list[i];
			list[i] = list[nIdxRandom];
			list[nIdxRandom] = temp;
		}
	}

	public static T GetRandomItem<T>(this List<T> list)
	{
		if (list.Count <= 0)
			return default(T);

		return list[GC.GetRandom(0, list.Count)];
	}

	public static void Resize<T>(this List<T> list, int size, T initValue)
	{
		int cur = list.Count;
		if (size < cur)
			list.RemoveRange(size, cur - size);
		else if (size > cur)
		{
			if (size > list.Capacity)
				list.Capacity = size;
			list.AddRange(System.Linq.Enumerable.Repeat(initValue, size - cur));
		}
	}

	public static void Resize<T>(this List<T> list, int size) where T : new()
	{
		Resize(list, size, new T());
	}

#endregion

#region // ----- Dictionary ----- //

#region Dictionary.LoopLinear - Dictionary enumerator 내 foreach에 의한 boxing, unboxing 차단 메소드, 4.x에선 미사용

	public static void LoopLinear<TKey, TValue>(this Dictionary<TKey, TValue> dict, System.Action<TValue> act)
	{
		List<TValue> listValue = new List<TValue>(dict.Values);
		for (int i = 0; i < listValue.Count; ++i)
			act(listValue[i]);
	}

	/// <summary> bool을 반환하는 Func로 반복문 제어 가능 </summary>
	/// <returns> break를 통해 중단되었을 경우 false </returns>
	public static bool LoopLinear<TKey, TValue>(this Dictionary<TKey, TValue> dict, System.Func<TValue, bool> act, bool isBreak = true)
	{
		List<TValue> listValue = new List<TValue>(dict.Values);
		for (int i = 0; i < listValue.Count; ++i)
		{
			if (!act(listValue[i]))
			{
				if (isBreak)
					return false;
				else
					continue;
			}
		}
		return true;
	}

	public static void LoopLinear<TKey, TValue>(this Dictionary<TKey, TValue> dict, System.Action<TKey> act)
	{
		List<TKey> listKey = new List<TKey>(dict.Keys);
		for (int i = 0; i < listKey.Count; ++i)
			act(listKey[i]);
	}

	/// <summary> bool을 반환하는 Func로 반복문 제어 가능 </summary>
	/// <returns> break를 통해 중단되었을 경우 false </returns>
	public static bool LoopLinear<TKey, TValue>(this Dictionary<TKey, TValue> dict, System.Func<TKey, bool> act, bool isBreak = true)
	{
		List<TKey> listKey = new List<TKey>(dict.Keys);
		for (int i = 0; i < listKey.Count; ++i)
		{
			if (!act(listKey[i]))
			{
				if (isBreak)
					return false;
				else
					continue;
			}
		}
		return true;
	}

	public static void LoopLinear<TKey, TValue>(this Dictionary<TKey, TValue> dict, System.Action<TKey, TValue> action)
	{
		LoopLinear(dict, key => action(key, dict[key]));
	}

	/// <summary> bool을 반환하는 Func로 반복문 제어 가능 </summary>
	/// <returns> break를 통해 중단되었을 경우 false </returns>
	public static bool LoopLinear<TKey, TValue>(this Dictionary<TKey, TValue> dict, System.Func<TKey, TValue, bool> act, bool isBreak = true)
	{
		return LoopLinear(dict, key => act(key, dict[key]), isBreak);
	}

	#endregion

	// ref : https://stackoverflow.com/questions/2974519/generic-constraints-where-t-struct-and-where-t-class
	public class RequireStruct<T> where T : struct { private RequireStruct() { } }
	public class RequireClass<T> where T : class { private RequireClass() { } }

#region Dictionary.GetSafe - 값 획득, 실패 시 추가

	public static TDerive GetSafe<TKey, TValue, TDerive>(this Dictionary<TKey, TValue> dict, TKey key, TValue defValue = null, RequireClass<TDerive> PleaseIgnoreThisParameter = null) 
		where TValue : class 
		where TDerive : class, TValue, new()
	{
		if (!dict.ContainsKey(key))
			dict.Add(key, defValue == null ? new TDerive() : defValue);

		return (TDerive)dict[key];
	}

	public static TValue GetSafe<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue defValue = default(TValue), RequireStruct<TValue> PleaseIgnoreThisParameter = null) 
		where TValue : struct
	{
		if (!dict.ContainsKey(key))
			dict.Add(key, defValue);

		return dict[key];
	}

	public static TValue GetSafe<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue defValue = null, RequireClass<TValue> PleaseIgnoreThisParameter = null) 
		where TValue : class, new()
	{
		if (!dict.ContainsKey(key))
			dict.Add(key, defValue == null ? new TValue() : defValue);

		return dict[key];
	}

	public static TValue[] GetSafe<TKey, TValue>(this Dictionary<TKey, TValue[]> dict, TKey key, TValue[] defValue = default(TValue[]), RequireStruct<TValue> PleaseIgnoreThisParameter = null) 
		where TValue : struct
	{
		if (!dict.ContainsKey(key))
			dict.Add(key, defValue);

		return dict[key];
	}

#endregion

#region Dictionary.GetDef - 값 획득, 실패 시 Default 반환

	public static TDerive GetDef<TKey, TValue, TDerive>(this Dictionary<TKey, TValue> dict, TKey key, TValue defValue = null, RequireClass<TDerive> PleaseIgnoreThisParameter = null)
		where TValue : class
		where TDerive : class, TValue
	{
		if (!dict.ContainsKey(key))
			return (TDerive)defValue;

		return (TDerive)dict[key];
	}

	public static TValue GetDef<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue defValue = default(TValue), RequireStruct<TValue> PleaseIgnoreThisParameter = null)
		where TValue : struct
	{
		if (!dict.ContainsKey(key))
			return defValue;

		return dict[key];
	}

	public static TValue GetDef<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue defValue = null, RequireClass<TValue> PleaseIgnoreThisParameter = null)
		where TValue : class
	{
		if (!dict.ContainsKey(key))
			return defValue;

		return dict[key];
	}

#endregion

#region Dictionary.SetSafe - 값 설정, 실패 시 추가

	public static TDerive SetSafe<TKey, TValue, TDerive>(this Dictionary<TKey, TValue> dict, TKey key, TValue value, RequireClass<TDerive> PleaseIgnoreThisParameter = null)
		where TValue : class
		where TDerive : class, TValue
	{
		if (!dict.ContainsKey(key))
		{
			dict.Add(key, value);
			return (TDerive)value;
		}

		return (TDerive)(dict[key] = value);
	}

	public static TValue SetSafe<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue value, RequireStruct<TValue> PleaseIgnoreThisParameter = null)
		where TValue : struct
	{
		if (!dict.ContainsKey(key))
		{
			dict.Add(key, value);
			return value;
		}

		return dict[key] = value;
	}

	public static TValue SetSafe<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue value, RequireClass<TValue> PleaseIgnoreThisParameter = null)
		where TValue : class
	{
		if (!dict.ContainsKey(key))
		{
			dict.Add(key, value);
			return value;
		}

		return dict[key] = value;
	}

#endregion

#endregion
}
