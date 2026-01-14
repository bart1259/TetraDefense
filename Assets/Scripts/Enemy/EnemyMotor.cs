using UnityEngine;

public class EnemyMotor : MonoBehaviour
{
    public GridPath Path { get { return _enemyPath; } }
    public float Speed { get { return _speed; } }
    public float Progress { get { return _currentPathIndex; } }
    public Transform EnemyGraphics;

    private GridPath _enemyPath;
    private float _speed;
    private int _currentPathIndex = 0;
    private EnemySO _enemyData;
    private Transform _goalLocation;
    private Vector3 _goalPosition;

    public void SetPath(GridPath path, Transform goalLocation)
    {
        _currentPathIndex = 0;
        _enemyPath = path;
        _goalPosition = new Vector3(_enemyPath.Cells[_currentPathIndex].X, 0.0f, _enemyPath.Cells[_currentPathIndex].Y);
        _goalLocation = goalLocation;
        _enemyData = GetComponent<EnemySOHolder>().EnemySOData;
        _speed = _enemyData.Speed;
    }

    public void PauseMovement()
    {
        _speed = 0.0f;
    }

    public void Update()
    {
        if (_enemyPath != null)
        {
            // Move towards that position
            Vector3 direction = (_goalPosition - transform.position).normalized;
            transform.position += direction * _speed * Time.deltaTime;
            EnemyGraphics.forward = direction;

            Vector3 distToTarget = _goalPosition - transform.position;
            if (distToTarget.magnitude < 0.1f)
            {


                // Move to next path index
                _currentPathIndex++;
                if (_currentPathIndex < _enemyPath.Cells.Count)
                {
                    _goalPosition = new Vector3(_enemyPath.Cells[_currentPathIndex].X, 0.0f, _enemyPath.Cells[_currentPathIndex].Y);
                }
                else if (_currentPathIndex >= _enemyPath.Cells.Count)
                {
                    _goalPosition = _goalLocation.position;

                    if ((transform.position - _goalPosition).magnitude < 0.1f)
                    {
                        // Reached goal
                        GameStateManager.GetInstance().TakeDamage(1);
                        Destroy(gameObject);
                    }
                }
            }
        }
    }
}
