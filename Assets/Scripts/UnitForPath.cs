using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitForPath : MonoBehaviour
{
    const float minPathUpdateTime = 0.2f;
    const float pathUpdateMoveThreshold = 0.5f;

    public Transform target;
    public float speed = 20f, turnSpeed = 3f;
    public int turnRadius = 5;

    public bool drawPathInEditor = false;

    private Path path;

    private void Start()
    {
        StartCoroutine("UpdatePath");
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

    private IEnumerator UpdatePath()
    {
        if (Time.timeSinceLevelLoad < 0.3f)
            yield return new WaitForSeconds(0.3f);

        PathRequestManager.RequestPath(transform.position, target.position, OnPathFound);

        var sqrMoveThreshold = pathUpdateMoveThreshold * pathUpdateMoveThreshold;
        Vector3 targetPosOld = target.position;

        while (true)
        {
            yield return new WaitForSeconds(minPathUpdateTime);
            //only request a new path if the target has moved farther away
            if((target.position - targetPosOld).sqrMagnitude > sqrMoveThreshold)
                PathRequestManager.RequestPath(transform.position, target.position, OnPathFound);
        }
    }

    private IEnumerator FollowPath()
    {
        bool followingPath = true;
        int pathIndex = 0;
        transform.LookAt(path.lookPoints[0]);

        while (followingPath)
        {
            Vector2 position = new Vector2(transform.position.x, transform.position.z);
            while (path.turnBoundaries[pathIndex].HasCrossedLine(position))
            {
                if (pathIndex == path.finishLineIndex)
                { 
                    followingPath = false;
                    break;
                }
                else
                    pathIndex++;
            }

            if(followingPath)
            {
                Quaternion targetRotation = Quaternion.LookRotation(path.lookPoints[pathIndex] - transform.position);

                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
                transform.Translate(Vector3.forward * Time.deltaTime * speed, Space.Self);
            }
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
