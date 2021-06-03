using UnityEngine;
using System.Collections;

public class PathWalker : MonoBehaviour
{
    private Vector2[] path = new Vector2[0];
    private int currentNode = 0;
    [SerializeField] private float positionEpsilon = 1f;
    public System.Action OnComplete = delegate { };

    public void SetPathAndStart(Vector2[] path)
    {
        this.path = path;
        this.currentNode = 0;
        StartCoroutine(Move());
    }

    public void Stop()
    {
        this.path = new Vector2[0];
        this.currentNode = 0;
        StopCoroutine(Move());
    }

    public int GetCurrentNode()
    {
        return currentNode;
    }

    public Vector3 GetCurrentNodePosition()
    {
        if (path == null || path.Length == 0)
            return new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        else
            return new Vector3(path[currentNode].x, 0, path[currentNode].y);
    }

    private IEnumerator Move()
    {
        if (path == null || path.Length <= 0)
        {
            yield break;
        }

        while (path != null && path.Length > 0 && currentNode < path.Length)
        {
            if ((transform.position - PathNodeToPosition(currentNode)).sqrMagnitude < positionEpsilon * positionEpsilon)
            {
                currentNode = Mathf.Clamp(currentNode + 1, 0, path.Length - 1);
            }    

            yield return null;
        }

        OnComplete();
        this.path = new Vector2[0];
        currentNode = 0;
    }

    private Vector3 PathNodeToPosition(int node)
    {
        return new Vector3(path[node].x, transform.position.y, path[node].y);
    }
}
