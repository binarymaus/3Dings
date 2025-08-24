using UnityEngine;

public class InputManagerTest : MonoBehaviour
{
    public Camera mainCamera;

    protected Vector2 m_StartingTouch;
	protected bool m_IsSwiping = false;

    void Update()
    {
        // Use touch input on mobile
        if (Input.touchCount == 1)
        {
            Debug.Log("Touch detected");
			if (m_IsSwiping)
            {
                Vector2 diff = Input.GetTouch(0).position - m_StartingTouch;

                // Put difference in Screen ratio, but using only width, so the ratio is the same on both
                // axes (otherwise we would have to swipe more vertically...)
                diff = new Vector2(diff.x / Screen.width, diff.y / Screen.width);
                Debug.Log($"Swipe diff: {diff}, magnitude: {diff.magnitude}");
                if (diff.magnitude > 0.01f) //we set the swip distance to trigger movement to 1% of the screen width
                {
                    if (Mathf.Abs(diff.y) > Mathf.Abs(diff.x))
                    {
                        if (diff.y < 0)
                        {
                            Debug.Log("Swipe Down");
                        }
                        else
                        {
                            Debug.Log("Swipe Up");
                        }
                    }
                    else if (diff.x < 0)
                    {
                        Debug.Log("Swipe Left");
                    }
                    else
                    {
                        Debug.Log("Swipe Right");
                    }

                    m_IsSwiping = false;
                }
            }

        	// Input check is AFTER the swip test, that way if TouchPhase.Ended happen a single frame after the Began Phase
			// a swipe can still be registered (otherwise, m_IsSwiping will be set to false and the test wouldn't happen for that began-Ended pair)
			if (Input.GetTouch(0).phase == TouchPhase.Began)
			{
                Debug.Log("TouchPhase.Began");
				m_StartingTouch = Input.GetTouch(0).position;
				m_IsSwiping = true;
			}
			else if (Input.GetTouch(0).phase == TouchPhase.Ended)
			{
                Debug.Log("TouchPhase.Ended");
				m_IsSwiping = false;
			}
        }
    }
}