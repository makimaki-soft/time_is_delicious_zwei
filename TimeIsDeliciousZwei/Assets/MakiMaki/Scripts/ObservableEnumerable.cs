using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;

public class ObservableEnumerator<T> : IEnumerator<T>
{
    private IEnumerator<T> _enumerator;
    public IEnumerator<T> Enumerator
    {
        set { _enumerator = value; }
        get { return _enumerator; }
    }

    private Subject<bool> _hasNextSubject = new Subject<bool>();
    public IObservable<bool> HasNextSubject
    {
        get { return _hasNextSubject; }
    }

    T IEnumerator<T>.Current
    {
        get
        {
            return _enumerator.Current;
        }
    }

    object IEnumerator.Current
    {
        get
        {
            return _enumerator.Current;
        }
    }

    bool IEnumerator.MoveNext()
    {
        var hasNext = _enumerator.MoveNext();
        _hasNextSubject.OnNext(hasNext);
        return hasNext;
    }

    void IEnumerator.Reset()
    {
        _enumerator.Reset();
    }

    void IDisposable.Dispose()
    {
        _hasNextSubject.OnNext(false);
        _enumerator.Dispose();
    }
}

public class ObservableEnumerable<T> : IEnumerable<T>
{
    public ObservableEnumerable(ICollection<T> src)
    {
        _srcEnumerable = src;
        _observableEnum = new ObservableEnumerator<T>();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        _observableEnum.Enumerator = _srcEnumerable.GetEnumerator();
        return _observableEnum;
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        _observableEnum.Enumerator = _srcEnumerable.GetEnumerator();
        return _observableEnum;
    }

    public IObservable<T> IterationAsObservable
    {
        get
        {
            return _observableEnum.HasNextSubject
                .TakeWhile(hasNext =>hasNext)
                .Select(_ => _observableEnum.Enumerator.Current)
                .Publish().RefCount();
        }
    }

    public int Count
    {
        get { return _srcEnumerable.Count; }
    }

    ICollection<T> _srcEnumerable;
    ObservableEnumerator<T> _observableEnum;
}

