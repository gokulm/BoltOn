IAppClock/AppClock
------------------------
There are instances where you have to use static properties DateTime.Now or DateTimeOffset.UtcNow, which makes hard to unit test, in those instances you could inject [`IAppClock`](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn/Utilities/AppClock.cs)
