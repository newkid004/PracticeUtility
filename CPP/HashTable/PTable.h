#pragma once

#include <list>
#include <cassert>
#include <cmath>

template<typename TKey, typename TValue>
class PTable
{
public:
	struct Node
	{
		TKey key;
		TValue value;
	};

private:
	static const size_t DEFAULT_CAP = 16u;

	using TBucket = typename std::list<Node>;
	using TBucketIter = typename std::list<Node>::iterator;

	using PHashFunction = int(*)(TKey);

public:
	PTable(size_t cap = 0) :
		_nCountItem(0),
		_nCountBucket(0),
		_dataTable(nullptr)
	{
		Rebucket(cap);
	}

	~PTable()
	{
		delete[](_dataTable);
	}

private:
	size_t _nCountItem;
	size_t _nCountBucket;

	std::list<Node>* _dataTable;
	
public:
	size_t CountItem() { return _nCountItem; }
	size_t CountBucket() { return _nCountBucket; }

public:
	bool Add(TKey key, TValue value)
	{
		return false == FindAndRun(key, [&](bool isFind, TBucket& bucket, TBucketIter& findNode)-> void
		{
			assert(!isFind && "중복 Key 추가됨");

			bucket.push_back(Node{ key, value });
			++_nCountItem;

			if (CheckSaturation(bucket))
			{
				Rebucket(_nCountBucket * 2u);
			}
		});
	}

	bool IsContains(TKey key)
	{
		size_t hashCode = KeyToHash(key);
		std::list<Node>& bucket = _dataTable[hashCode % _nCountBucket];

		auto buckIter = bucket.begin();
		auto buckEnd = bucket.end();

		while (buckIter != buckEnd)
		{
			if (buckIter->key == key)
				return true;

			++buckIter;
		}

		return false;
	}

	bool Search(TKey key, TValue& value)
	{
		return FindAndRun(key, [&value](bool isFind, TBucket& bucket, TBucketIter& findNode)-> void
		{
			if (isFind)
				value = findNode->value;
		});
	}

	bool Delete(TKey key)
	{
		return FindAndRun(key, [this](bool isFind, TBucket& bucket, TBucketIter& findNode)-> void
		{
			if (isFind)
			{
				bucket.erase(findNode);
				--_nCountItem;
			}
		});
	}

	void Clear()
	{
		LoopBucket([](int index, std::list<Node>& bucket)
		{
			bucket.clear();
			return true;
		});
	}

	//! Loop by Index
	//! bool (Key, Value)
	template<typename F>
	void Loop(F&& func)
	{
		bool isContinue = true;

		for (size_t i = 0; i < _nCountBucket && isContinue; ++i)
		{
			std::list<Node>& bucket = _dataTable[i];

			auto buckIter = bucket.begin();
			auto buckEnd = bucket.end();

			while (buckIter != buckEnd && isContinue)
			{
				isContinue = func(buckIter->key, buckIter->value);
				++buckIter;
			}
		}
	}

	//! Loop by Bucket
	//! bool (bucketIndex, bucket)
	template<typename F>
	void LoopBucket(F&& func)
	{
		bool isContinue = true;

		for (size_t i = 0; i < _nCountBucket && isContinue; ++i)
		{
			std::list<Node>& bucket = _dataTable[i];
			
			if (0u < bucket.size())
				isContinue = func(i, bucket);
		}
	}

private:
	size_t KeyToHash(TKey key)
	{
		return std::hash<TKey>{}(key);
	}

	//! void (isFind, bucket, node)
	template<typename F>
	//x template<typename F, typename = std::enable_if_t<std::_Invoke_traits_nonzero<F, TBucket&, TBucketIter>::_Is_invocable>>
	//x template<typename F, typename = std::enable_if_t<std::is_same_v<F, PActionFunction>>>
	bool FindAndRun(TKey key, F && func)
	{
		size_t hashCode = KeyToHash(key);
		std::list<Node>& bucket = _dataTable[hashCode % _nCountBucket];

		auto buckIter = bucket.begin();
		auto buckEnd = bucket.end();

		while (buckIter != buckEnd)
		{
			if (buckIter->key == key)
			{
				// 탐색 성공
				func(true, bucket, buckIter);
				return true;
			}

			++buckIter;
		}

		// 탐색 실패
		func(false, bucket, buckIter);
		return false;
	}

	bool CheckSaturation(TBucket& lastInsertionBucket)
	{
		return std::sqrt(_nCountBucket) < lastInsertionBucket.size(); // [버켓 내부의 아이템] < [총 버켓 개수의 제곱근] : 재조정
	}

	void Rebucket(size_t cap)
	{
		if (cap == 0u)
			cap = DEFAULT_CAP;

		if (_dataTable == nullptr)
		{
			_dataTable = new std::list<Node>[cap];
		}
		else
		{
			std::list<Node>* tempTable = _dataTable;
			size_t nPrevCount = _nCountBucket;

			_dataTable = new std::list<Node>[cap];
			_nCountBucket = cap;

			for (size_t i = 0; i < nPrevCount; ++i)
			{
				std::list<Node>& bucket = tempTable[i];

				auto buckIter = bucket.begin();
				auto buckEnd = bucket.end();

				while (buckIter != buckEnd)
				{
					this->Add(buckIter->key, buckIter->value);
					++buckIter;
				}
			}

			delete[](tempTable);
		}

		_nCountBucket = cap;
	}
};
