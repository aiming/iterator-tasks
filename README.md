# Iterator Tasks

Iterator Tasks is a iterator-based coroutine class library.

## Asynchronous Operations on Unity

One of the purpose of Iterator Tasks is to simplify asynchronous operations on Unity 3D game engine. Unity is built on Mono, open source .NET development framework. It however uses older version which correspond approximately to .NET Framework 3.5 and C# 3.0. Thus, there is no Task class and async/await support.

Alternatively, Unity provides iterator-based coroutine framework for asynchronous operation. For example, following code awaits web access completion without blocking a thread by using a yield return statement.

```c#
using UnityEngine;
using System.Collections;

public class example : MonoBehaviour
{
    public string url = "http://images.earthcam.com/ec_metros/ourcams/fridays.jpg";
    IEnumerator Start()
    {
        WWW www = new WWW(url);
        yield return www;
        renderer.material.mainTexture = www.texture;
    }
}
```

However, there are not so many developers understanding usage of iterator block (yield return statement). Thus, it is slightly difficult to use iterator block for asynchronous operation, especially when a return value of the asynchronous operation is needed for subsequent tasks.

Because of this, Iterator Tasks wraps a  iterator-based coroutine with a class which resembles Task class in .NET 4.

## Example Usage

Here is an example of Task class in Iterator Tasks.

```c#
var task = new Task<double>(c => Coroutines.F1Async(x, c))
    .ContinueWith<string>(Coroutines.F2Async)
    .ContinueWith<int>(Coroutines.F3Async);

task.OnComplete(t => Console.WriteLine(t.Result));
```

Where F1Async, F2Async, F3Async are iterator-based coroutine as follows:

```c#
public static System.Collections.IEnumerator F1Async(double x, Action<double> completed)
{
  for (int i = 0; i < 5; i++) yield return null;
	var result = F1(x);
	completed(result);
}

public static System.Collections.IEnumerator F2Async(double x, Action<string> completed)
{
	for (int i = 0; i < 5; i++) yield return null;
	var result = F2(x);
	completed(result);
}

public static IEnumerator F3Async(string s, Action<int> completed)
{
	for (int i = 0; i < 5; i++) yield return null;
	var result = F3(s);
	completed(result);
}
```

# [License](https://github.com/aiming/iterator-tasks/blob/master/LICENSE.md)

# Contributing
1. Fork it
2. Create your feature branch (`git checkout -b my-new-feature`)
3. Commit your changes (`git commit -am 'Add some feature'`)
4. Push to the branch (`git push origin my-new-feature`)
5. Create new Pull Request
