using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace UnityStandardAssets.Vehicles.Car
{
    [RequireComponent (typeof(CarController))]
    public class CarAIControl : MonoBehaviour
    {
        //Brake Condition F5, F6, F7키로 변경가능
        public enum BrakeCondition
        {
            //계속 직진 F5
            NeverBrake,
            //브레이크 F7
            TargetDirectionDifference,
            //핸들 꺾음 F6
            TargetDistance,
        }

        /*
         *기존값들
        */
        [SerializeField] [Range (0, 1)] private float m_CautiousSpeedFactor = 0.05f;
        // percentage of max speed to use when being maximally cautious
        [SerializeField] [Range (0, 180)] private float m_CautiousMaxAngle = 50f;
        // angle of approaching corner to treat as warranting maximum caution
        [SerializeField] private float m_CautiousMaxDistance = 100f;
        // distance at which distance-based cautiousness begins
        [SerializeField] private float m_CautiousAngularVelocityFactor = 30f;
        // how cautious the AI should be when considering its own current angular velocity (i.e. easing off acceleration if spinning!)
        [SerializeField] private float m_SteerSensitivity = 0.05f;
        // how sensitively the AI uses steering input to turn to the desired direction
        [SerializeField] private float m_AccelSensitivity = 0.04f;
        // How sensitively the AI uses the accelerator to reach the current desired speed
        [SerializeField] private float m_BrakeSensitivity = 1f;
        // How sensitively the AI uses the brake to reach the current desired speed
        [SerializeField] private float m_LateralWanderDistance = 3f;
        // how far the car will wander laterally towards its target
        [SerializeField] private float m_LateralWanderSpeed = 0.1f;
        // how fast the lateral wandering will fluctuate
        [SerializeField] [Range (0, 1)] private float m_AccelWanderAmount = 0.1f;
        // how much the cars acceleration will wander
        [SerializeField] private float m_AccelWanderSpeed = 0.1f;
        // what should the AI consider when accelerating/braking?
        [SerializeField] private bool m_Driving;
        // whether the AI is currently actively driving or stopped.
        [SerializeField] private Transform m_Target;
        // 'target' the target object to aim for.
        [SerializeField] private bool m_StopWhenTargetReached;
        // should we stop driving when we reach the target?
        [SerializeField] private float m_ReachTargetThreshold = 2;
        // proximity to target to consider we 'reached' it, and stop driving.
        private float m_RandomPerlin;
        // A random value for the car to base its wander on (so that AI cars don't all wander in the same pattern)
        private CarController m_CarController;
        // Reference to actual car controller we are controlling
        private float m_AvoidOtherCarTime;
        // time until which to avoid the car we recently collided with
        private float m_AvoidOtherCarSlowdown;
        // how much to slow down due to colliding with another car, whilst avoiding
        private float m_AvoidPathOffset;
        // direction (-1 or 1) in which to offset path to avoid other car, whilst avoiding
        private Rigidbody m_Rigidbody;
        /*
         * 기존값 종료
         */

        //주행 방식 변경
        public BrakeCondition m_BrakeCondition = BrakeCondition.TargetDirectionDifference;

        private void Awake ()
        {
            // get the car controller reference
            m_CarController = GetComponent<CarController> ();

            // give the random perlin a random value
            m_RandomPerlin = Random.value * 100;

            m_Rigidbody = GetComponent<Rigidbody> ();
        }

        //Open_CV_Var로 부터 값 받아옴
        private float steering_var = 0;

        //회전 각 변경 - Custom
        public void Change_Value_Steering(Vector2 direction)
        {
            if (Mathf.Abs(direction.x) < 12 || direction == null)
            {
                steering_var = 0;
                return;
            }

           // float slope = (direction.y / 2) / direction.x;

            if (direction.x < 0)
            {
                steering_var = -0.5f;
                
            }
            else
            {
                steering_var = 0.5f;
                
            }

            Debug.Log("Change_Steering :" +steering_var);
        }

        bool isTurn = false;
        int Turn_Count = 0;

        void move_next()
        {
            m_CarController.Move(-0.3f, 1.0f, 0, 0 );
        }

        
        //Fixed - Custom
        private void FixedUpdate ()
        {
            // 주행방식 변경
            if (m_Target == null || !m_Driving) {
                m_CarController.Move (steering_var, 1f, 0, 0);
            }
            else
            {
                Vector3 fwd = transform.forward;
                if (m_Rigidbody.velocity.magnitude > m_CarController.MaxSpeed * 0.1f) {
                    fwd = m_Rigidbody.velocity;
                }

                float desiredSpeed = m_CarController.MaxSpeed;

                //주행방식에 따른 다른 행동 지시
                switch (m_BrakeCondition) {
                    case BrakeCondition.TargetDistance:
                    Debug.Log("work - handle change");
                        if (!isTurn)
                        {
                            m_CarController.Move(0.4f, 1.0f, 0, 0);
                            m_Driving = true;
                            Turn_Count++;
                            if (Turn_Count > 30)
                            {
                                Turn_Count = 0;
                                isTurn = true;
                            }
                        }
                        else
                        {
                            Turn_Count++;
                            if(Turn_Count > 40 && Turn_Count <55)
                            {
                                m_CarController.Move(0.2f, 1.0f, 0, 0);
                             
                            }else if(Turn_Count >= 55)
                            {
                                Turn_Count = 0;
                                isTurn = false;
                                m_Target = null;
                                m_Driving = false;
                            }
                            move_next();
                        }
                    break;
                    
                case BrakeCondition.TargetDirectionDifference:
                    Debug.Log("work - brake");
                                                
                        m_CarController.Move(0, 0, 500.0f, 500.0f);
                        m_Driving = true;
                    break;


                    case BrakeCondition.NeverBrake:
                        Debug.Log("work - neverbrake");
                        
                        m_Driving = true;
                        break;
                }

                
            }
        }

        //충돌상황에서의 처리 - Custom
        private void OnCollisionStay (Collision col)
        {
            if (col.rigidbody != null) {
                var otherAI = col.rigidbody.GetComponent<CarAIControl> ();
                if (otherAI != null) {
            
                    m_AvoidOtherCarTime = Time.time + 1;
                                
                    if (Vector3.Angle (transform.forward, otherAI.transform.position - transform.position) < 90) {
                        m_AvoidOtherCarSlowdown = 0.5f;
                    } else {
                        m_AvoidOtherCarSlowdown = 1;
                    }

                    var otherCarLocalDelta = transform.InverseTransformPoint (otherAI.transform.position);
                    float otherCarAngle = Mathf.Atan2 (otherCarLocalDelta.x, otherCarLocalDelta.z);
                    m_AvoidPathOffset = m_LateralWanderDistance * -Mathf.Sign (otherCarAngle);
                }
            }
        }

        //카메라로부터 타겟 받아옴 - Custom
        public void SetTarget (Transform target)
        {
            
                m_Target = target;
                m_Driving = true;
                Debug.Log("settar - work");
            
        }
    }
}