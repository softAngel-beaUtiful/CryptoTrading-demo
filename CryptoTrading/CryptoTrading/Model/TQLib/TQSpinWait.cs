using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoTrading.TQLib
{
    class TQSpinWait
    {
        private bool _Stop = false;

        public Func<bool> WaitCondition;
        public TimeSpan WaitTimeOut = TimeSpan.FromDays(10);
        public event Action Handler;

        private long meetTimes = 0;
        public long MeetTimes
        {
            get { return meetTimes; }
        }

        public long MaxMeetTimes = 0;

        public TQSpinWait(Func<bool> Condition)
        {
            var WaitConditionExpression = Expression.OrElse(
                FuncToExpression(() => _Stop).Body,
                FuncToExpression(Condition).Body
            );

            var WaitConditionLambda = Expression.Lambda<Func<bool>>(WaitConditionExpression);

            WaitCondition = WaitConditionLambda.Compile();
        }
        private Expression<Func<bool>> FuncToExpression(Func<bool> f)
        {
            return () => f();
        }

        public void Start()
        {
            if (WaitCondition != null && Handler != null)
            {
                Thread _Thread = new Thread(ThreadEvent);

                _Thread.IsBackground = true;

                _Thread.Start();
            }
        }

        private void ThreadEvent()
        {
            while
            (
                (MaxMeetTimes == 0 || meetTimes < MaxMeetTimes) &&
                !_Stop &&
                 Handler != null
            )
            {
                SpinWait.SpinUntil(WaitCondition, WaitTimeOut);

                if (_Stop) return;

                if (meetTimes >= long.MaxValue)
                    meetTimes = 0;

                meetTimes++;

                if (Handler != null) // 可能在 等待 WaitCondition 的时候，Handler 被注销了。
                    Handler();
            }
        }

        public void Stop()
        {
            _Stop = true;

            //_Thread.Abort();
        }
    }
}
