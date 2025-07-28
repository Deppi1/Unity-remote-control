using UnityEditor;
using UnityEngine;

public class GridChecker : MonoBehaviour
{
    [SerializeField] private Vector2 _rectangleSize;
    [SerializeField] private int _cellsInLength;
    [SerializeField] private int _cellsInWidth;
    [SerializeField] private GameObject _gameObject;
    [SerializeField] private int _objectPosition;

    private void Update()
    {
        UpdateObjectPosition();
    }
    private void OnDrawGizmos()
    {
        float cellWidth = _rectangleSize.x / _cellsInWidth;
        float cellHeight = _rectangleSize.y / _cellsInLength;

        Vector3 startPos = transform.position - new Vector3(_rectangleSize.x / 2, 0, _rectangleSize.y / 2);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(_rectangleSize.x, 0, _rectangleSize.y));

        Gizmos.color = Color.green;
        for (int y = 0; y < _cellsInLength; y++)
        {
            for (int x = 0; x < _cellsInWidth; x++)
            {
                Vector3 cellCenter = startPos + new Vector3(x * cellWidth + cellWidth / 2, 0, y * cellHeight + cellHeight / 2);
                Gizmos.DrawWireCube(cellCenter, new Vector3(cellWidth, 0, cellHeight));
                Handles.Label(cellCenter, ((y * _cellsInWidth) + x + 1).ToString());
            }
        }
    }

    private void UpdateObjectPosition()
    {
        float cellWidth = _rectangleSize.x / _cellsInWidth;
        float cellHeight = _rectangleSize.y / _cellsInLength;

        Vector3 startPos = transform.position - new Vector3(_rectangleSize.x / 2, 0, _rectangleSize.y / 2);

        Vector3 objPos = _gameObject.transform.position;

        int cellX = Mathf.FloorToInt((objPos.x - startPos.x) / cellWidth);
        int cellY = Mathf.FloorToInt((objPos.z - startPos.z) / cellHeight);

        if (cellX >= 0 && cellX < _cellsInWidth && cellY >= 0 && cellY < _cellsInLength)
        {
            _objectPosition = (cellY * _cellsInWidth) + cellX + 1;
        }
        else
        {
            _objectPosition = -1;
        }
    }
}
