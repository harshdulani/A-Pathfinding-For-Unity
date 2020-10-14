using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitForPath : MonoBehaviour
{
    public Transform target;
    public float speed = 20f;
    public int turnRadius = 5;

    public bool drawPathInEditor = false;

    private Path path;

    private void Start()
    {
        PathRequestManager.RequestPath(transform.position, target.position, OnPathFound);
    }

    public void OnPathFound(Vector3[] waypoints,  bool pathSuccess)
    {
        if (pathSuccess)
        {
            path = new Path(waypoints, transform.position, turnRadius);
            StopCoroutine("FollowPath");
            StartCoroutine("FollowPath");
        }
    }

    private IEnumerator FollowPath()
    {
        while (true)
        {
            yield return null;
        }
    }

    public void OnDrawGizmos()
    {
        if(drawPathInEditor)
            if(path != null)
            {
                path.DrawWithGizmos();
            }
    }
}
