using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitForPath : MonoBehaviour
{
    public Transform target;
    public float speed = 20f;

    public bool drawPathInEditor = false;

    private Vector3[] path;
    private int targetIndex;

    private void Start()
    {
        PathRequestManager.RequestPath(transform.position, target.position, OnPathFound);
    }

    public void OnPathFound(Vector3[] newPath,  bool pathSuccess)
    {
        if (pathSuccess)
        {
            path = newPath;
            StopCoroutine("FollowPath");
            StartCoroutine("FollowPath");
        }
    }

    private IEnumerator FollowPath()
    {
        Vector3 currenWaypoint = path[0];

        while (true)
        {
            if (transform.position == currenWaypoint)
            {
                targetIndex++;
                if(targetIndex >= path.Length)
                {
                    //prospect to reset path if unit needs to search for new path after old one has been completed/INT occured
                    //targetIndex = 0;
                    //path = new Vector3[0];
                    yield break;
                }
                currenWaypoint = path[targetIndex];
            }

            //Movement code goes here
            #region movement
            transform.position = Vector3.MoveTowards(transform.position, currenWaypoint, speed * Time.deltaTime);
            #endregion
            yield return null;
        }
    }

    public void OnDrawGizmos()
    {
        if(drawPathInEditor)
            if(path != null)
            {
                for (int i = targetIndex; i < path.Length; i++)
                {
                    Gizmos.color = Color.black;
                    Gizmos.DrawCube(path[i], Vector3.one);

                    if (i == targetIndex)
                        Gizmos.DrawLine(transform.position, path[i]);
                    else
                        Gizmos.DrawLine(path[i - 1], path[i]);
                }
            }
    }
}
