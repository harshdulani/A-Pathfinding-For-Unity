using System;
using System.Collections.Concurrent;
using System.Threading;
using UnityEngine;

public class PathRequestManager : MonoBehaviour
{
    //this class manages incoming requests from units that want paths,
    //so that all of them don't calculate paths at the same time and slow down the a single frame
    //split up these requests over multiple frames

    //ONE FRAME is processing only ONE PATH

    private ConcurrentQueue<PathResult> results = new ConcurrentQueue<PathResult>();

    private static PathRequestManager instance;
    private Pathfinding pathfinder;

    private void Awake()
    {
        instance = this;
        pathfinder = GetComponent<Pathfinding>();
    }

    private void Update()
    {
        if(results.Count > 0)
        {
            int resultsInQueue = results.Count;
            for (int i = 0; i < resultsInQueue; i++)
            {
                _ = results.TryDequeue(out PathResult result);
                result.callback(result.path, result.success);
            }
        }
    }

    public static void RequestPath(PathRequest request)
    {
        ThreadStart threadStart = delegate
        {
            //any calls made from this method and the methods it calls,
            //will now run on the separate thread that this function started
            //that will create problems because of its asynchronousity with Main thread (which runs update etc)
            instance.pathfinder.FindPath(request, instance.FinishedProcessingPath);
        };
        threadStart.Invoke();
    }

    public void FinishedProcessingPath(PathResult result)
    {
        results.Enqueue(result);
    }
}

public struct PathRequest
{
    public Vector3 pathStart;
    public Vector3 pathEnd;
    //it is a variable to store a method that you'll send on runtime
    public Action<Vector3[], bool> callback;

    //constructor for struct
    public PathRequest(Vector3 _start, Vector3 _end, Action<Vector3[], bool> _callback)
    {
        pathStart = _start;
        pathEnd = _end;
        callback = _callback;
    }
}

public struct PathResult
{
    public Vector3[] path;
    public bool success;
    public Action<Vector3[], bool> callback;

    public PathResult(Vector3[] path, bool success, Action<Vector3[], bool> callback)
    {
        this.path = path;
        this.success = success;
        this.callback = callback;
    }
}