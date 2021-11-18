using UnityEngine;
using System.Collections;

namespace VikingCrewDevelopment {
    public class RandomMovementBehaviour : MonoBehaviour {
        public Rect bounds = new Rect(-10, -10, 20, 20);
        public float speed = 1;
        Vector2 nextWaypoint;
        // Use this for initialization
        void Start() {
            nextWaypoint = GetNextWaypoint();
        }

        // Update is called once per frame
        void Update() {

            transform.position = Vector2.MoveTowards(transform.position, nextWaypoint, speed * Time.deltaTime);
            if (Vector2.Distance(transform.position, nextWaypoint) < 0.25) {
                nextWaypoint = GetNextWaypoint();
            }

        }

        Vector2 GetNextWaypoint() {
            return new Vector2(Random.Range(bounds.xMin, bounds.xMax), Random.Range(bounds.yMin, bounds.yMax));
        }
    }
}