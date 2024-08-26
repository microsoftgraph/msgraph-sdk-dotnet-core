using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Graph;

/// <summary>
/// Represents a collection of ordered <see cref="BatchRequestStep"/> objects.
/// </summary>
internal class BatchRequestContentSteps : IReadOnlyDictionary<string, BatchRequestStep>, IDictionary<string, BatchRequestStep>
{
    private readonly Dictionary<string, BatchRequestStep> _steps;

    private readonly List<string> _requestIds;

    /// <summary>
    /// Initializes a new instance of the <see cref="BatchRequestContentSteps"/> class which keeps track of the order of the steps.
    /// </summary>
    public BatchRequestContentSteps()
    {
        _steps = new Dictionary<string, BatchRequestStep>();
        _requestIds = new List<string>();
    }

    /// <summary>
    /// Gets or sets the <see cref="BatchRequestStep"/> with the specified key.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public BatchRequestStep this[string key]
    {
        get => _steps[key];
        set => Add(key, value);
    }

    /// <summary>
    /// Gets the keys in the collection.
    /// </summary>
    public ICollection<string> Keys => _requestIds;

    /// <summary>
    ///  Gets the values in the collection.
    /// </summary>
    public ICollection<BatchRequestStep> Values
    {
        get
        {
            List<BatchRequestStep> values = new List<BatchRequestStep>();
            foreach (var key in _requestIds)
            {
                values.Add(_steps[key]);
            }
            return values;
        }
    }

    /// <summary>
    /// Gets the number of elements in the collection.
    /// </summary>
    public int Count => _requestIds.Count;

    /// <summary>
    /// Gets a value indicating whether the collection is read-only.
    /// </summary>
    public bool IsReadOnly => true;

    IEnumerable<string> IReadOnlyDictionary<string, BatchRequestStep>.Keys => Keys.AsEnumerable();

    IEnumerable<BatchRequestStep> IReadOnlyDictionary<string, BatchRequestStep>.Values => Values.AsEnumerable();

    /// <summary>
    /// Adds a <see cref="BatchRequestStep"/> to the collection.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void Add(string key, BatchRequestStep value)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentNullException(nameof(key));

        _steps.Add(key, value);
        _requestIds.Add(key);
    }

    /// <summary>
    /// Adds a <see cref="BatchRequestStep"/> to the collection.
    /// </summary>
    /// <param name="item"></param>
    public void Add(KeyValuePair<string, BatchRequestStep> item)
    {
        Add(item.Key, item.Value);
    }

    /// <summary>
    /// Clears the collection.
    /// </summary>
    public void Clear()
    {
        _steps.Clear();
        _requestIds.Clear();
    }

    /// <summary>
    /// Determines whether the collection contains a specific value.
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool Contains(KeyValuePair<string, BatchRequestStep> item)
    {
        if (!_steps.ContainsKey(item.Key))
        {
            return false;
        }
        return _steps[item.Key] == item.Value;
    }

    /// <summary>
    /// Determines whether the collection contains a specific key.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool ContainsKey(string key)
    {
        return _steps.ContainsKey(key);
    }

    /// <summary>
    /// Copies the elements of the collection to an array, starting at a particular array index.
    /// </summary>
    /// <param name="array"></param>
    /// <param name="arrayIndex"></param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public void CopyTo(KeyValuePair<string, BatchRequestStep>[] array, int arrayIndex)
    {
        if (array == null)
            throw new ArgumentNullException(nameof(array));

        if (arrayIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(arrayIndex));

        if (array.Length - arrayIndex < _steps.Count)
            throw new ArgumentException("The number of elements in the source collection is greater than the available space from arrayIndex to the end of the destination array.");

        foreach (var key in _requestIds)
        {
            array[arrayIndex] = new KeyValuePair<string, BatchRequestStep>(key, _steps[key]);
            arrayIndex++;
        }
    }

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns></returns>
    public IEnumerator<KeyValuePair<string, BatchRequestStep>> GetEnumerator()
    {
        return new BatchRequestStepEnumerator(_requestIds, _steps);
    }

    /// <summary>
    /// Removes the <see cref="BatchRequestStep"/> with the specified key from the collection.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool Remove(string key)
    {
        if (_steps.ContainsKey(key))
        {
            _steps.Remove(key);
            _requestIds.Remove(key);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Removes the <see cref="BatchRequestStep"/> with the specified key from the collection.
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool Remove(KeyValuePair<string, BatchRequestStep> item)
    {
        return Remove(item.Key);
    }

    /// <summary>
    /// Tries to get the value associated with the specified key.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public bool TryGetValue(string key, out BatchRequestStep value)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentNullException(nameof(key));

        if (_steps.ContainsKey(key))
        {
            value = _steps[key];
            return true;
        }
        value = null;
        return false;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private sealed class BatchRequestStepEnumerator : IEnumerator<KeyValuePair<string, BatchRequestStep>>
    {
        private int _currentRequestIdIndex = -1;
        private readonly List<string> _requestIds;
        private readonly Dictionary<string, BatchRequestStep> _steps;

        public BatchRequestStepEnumerator(List<string> requestIds, Dictionary<string, BatchRequestStep> steps)
        {
            _requestIds = requestIds;
            _steps = steps;
        }

        public KeyValuePair<string, BatchRequestStep> Current
        {
            get {
                if (_currentRequestIdIndex < 0 || _currentRequestIdIndex >= _requestIds.Count)
                {
                    throw new InvalidOperationException();
                }
                var key = _requestIds[_currentRequestIdIndex];
                return new KeyValuePair<string, BatchRequestStep>(key, _steps[key]);
            }
        }
        object IEnumerator.Current => Current;

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public bool MoveNext()
        {

            _currentRequestIdIndex++;
            return _currentRequestIdIndex <= _requestIds.Count - 1;
        }

        public void Reset()
        {
            throw new NotSupportedException();
        }
    }
}
