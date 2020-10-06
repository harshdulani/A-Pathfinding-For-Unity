using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathRequestManager : MonoBehaviour
{
    //this class manages incoming requests from units that want paths,
    //so that all of them don't calculate paths at the same time and slow down the a single frame
    //split up these requests over multiple frames

    //ONE FRAME is processing only ONE PATH

    private Queue<PathRequest> pathRequestQueue = new Queue<PathRequest>();
    private PathRequest currentPathRequest;

    private static PathRequestManager instance;
    private Pathfinding pathfinder;

    private bool isProcessingPath;

    void Awake()
    {
        instance = this;
        pathfinder = GetComponent<Pathfinding>();
    }

    public static void RequestPath(Vector3 pathStart, Vector3 pathEnd, Action<Vector3[], bool> callback)
    {
        PathRequest newRequest = new PathRequest(pathStart, pathEnd, callback);
        instance.pathRequestQueue.Enqueue(newRequest);
        instance.TryProcessNext();
    }

    private void TryProcessNext()
    {
        //tries to see if were currently already processing a path
        //and if not, tells the class to process the newest one

        if(!isProcessingPath && pathRequestQueue.Count > 0)
        {
            currentPathRequest = pathRequestQueue.Dequeue();
            isProcessingPath = true;
            pathfinder.StartFindingPath(currentPathRequest.pathStart, currentPathRequest.pathEnd);
        }
    }

    public void FinishedProcessingPath(Vector3[] path, bool success)
    {
        currentPathRequest.callback(path, success);
        isProcessingPath = false;
        TryProcessNext();
    }

    private struct PathRequest
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
}