using System;
using System.Collections.Generic;
using UniRx;
using UniRx.Operators;

/**
 * 連続して入ってきたメッセージをQueueにため、nextに指定したストリームがonNextされるたびに１つずつメッセージを流す
 */
public class OneByOneObservable<T, S> : OperatorObservableBase<T>
{
    readonly IObservable<T> _source;
    readonly IObservable<S> _next;

    public OneByOneObservable(IObservable<T> source, IObservable<S> next)
        : base(source.IsRequiredSubscribeOnCurrentThread())
    {
        this._source = source;
        this._next = next;
    }

    protected override IDisposable SubscribeCore(IObserver<T> observer, IDisposable cancel)
    {
        // return source.Subscribe(new Boss(observer, cancel)).Run();
        return new OneByOne(this, observer, cancel).Run();
    }

    class OneByOne : OperatorObserverBase<T, T>
    {
        readonly OneByOneObservable<T, S> _parent;
        private Queue<T> _messageQueue;
        private bool _runnable;

        public OneByOne(OneByOneObservable<T, S> parent, IObserver<T> observer, IDisposable cancel)
            : base(observer, cancel)
        {
            this._parent = parent;
        }

        public override void OnNext(T value)
        {
            if (_runnable)
            {
                _runnable = false;
                base.observer.OnNext(value);
            }
            else
            {
                _messageQueue.Enqueue(value);
            }
        }

        public override void OnError(Exception error)
        {
            try
            {
                observer.OnError(error);
            }
            finally { Dispose(); }
        }

        public override void OnCompleted()
        {
            try
            {
                observer.OnCompleted();
            }
            finally { Dispose(); }
        }

        public IDisposable Run()
        {
            _messageQueue = new Queue<T>();
            _runnable = true;

            var sourceSubscription = _parent._source.Subscribe(this);
            var windowSubscription = _parent._next.Subscribe(_ =>
            {
                if (_messageQueue.Count > 0)
                {
                    base.observer.OnNext(_messageQueue.Dequeue());
                }
                else
                {
                    _runnable = true;
                }
            });

            return StableCompositeDisposable.Create(sourceSubscription, windowSubscription);
        }
    }
}

public static class OneByOneObservableExtension
{
    public static IObservable<T> OneByOne<T, S>(this IObservable<T> source, IObservable<S> next)
    {
        return new OneByOneObservable<T, S>(source, next);
    }
}