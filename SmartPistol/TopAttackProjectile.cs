using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace Cityrobo
{
    public class TopAttackProjectile : MonoBehaviour
    {
        public BallisticProjectile Projectile;
        [HideInInspector]
        //public Vector3 TargetPoint = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        public Vector3? TargetPoint = null;
        [HideInInspector]
        public Rigidbody TargetRB;

        public float TurningSpeed = 45f;

        public enum EAttackMode
        {
            Top,
            Direct
        }

        public EAttackMode AttackMode;


        //public float MaxHeightInFrontalAttack = 60f;

        public float TopAttackLaunchAngle = 60f;
        public float TopAttackAttackAngle = 45f;
        public float DirectAttackLaunchAngle = 45f;
        public float DirectAttackAttackAngle = 1.75f;

        public float MaxHeightInTopAttack = 160f;
        private enum AttackState
        {
            ClimbOut,
            AltitudeHold,
            Terminal
        }

        private AttackState _attackState;

        private float _startingDistanceToTarget;
        private float _startingHeight;

        private Vector3 _targetDirection;
        private float _targetHeight;
        private bool _doesAltitudeHolt = false;

        private float _altitudeHoldMaxDistance;
        private float _altitudeHoldCurrentDistance;

        private bool _debug = true;

#if !DEBUG

        public void Start()
        {
            float turnRadius = (180 * Projectile.m_velocity.magnitude) / (Mathf.PI * TurningSpeed);
            _startingHeight = Projectile.transform.position.y;
            

            Vector3 TargetPos = Vector3.zero;

            if (TargetRB != null) TargetPos = TargetRB.position;
            else if (TargetPoint != null) TargetPos = TargetPoint.Value;
            else Destroy(this);

            float _deltaHeight = TargetPos.y - _startingHeight;
            //_startingDistanceToTarget = Vector3.Distance(TargetPos, Projectile.transform.position);

            float _deltaXZ = Vector3.ProjectOnPlane(TargetPos - Projectile.transform.position, Vector3.up).magnitude;


            if (AttackMode == EAttackMode.Top)
            {
                float _heightCorrection = _deltaHeight / Mathf.Tan(TopAttackAttackAngle * Mathf.Deg2Rad);
                _startingDistanceToTarget = _deltaXZ + _heightCorrection;
                float deltaAngle = TopAttackLaunchAngle - Vector3.Angle(Projectile.transform.forward, Vector3.ProjectOnPlane(Projectile.transform.forward,Vector3.up));

                _targetDirection = Quaternion.AngleAxis(-deltaAngle, Projectile.transform.right) * Projectile.transform.forward;

                _targetHeight = _startingDistanceToTarget * (Mathf.Tan(TopAttackLaunchAngle * Mathf.Deg2Rad) * Mathf.Tan(TopAttackAttackAngle * Mathf.Deg2Rad)) / (Mathf.Tan(TopAttackLaunchAngle * Mathf.Deg2Rad) + Mathf.Tan(TopAttackAttackAngle * Mathf.Deg2Rad));

                if (_debug) Debug.Log($"First _targetHeight: {_targetHeight}");
                if (_deltaHeight > 0 && _targetHeight > MaxHeightInTopAttack + _deltaHeight)
                {
                    _doesAltitudeHolt = true;
                    _targetHeight = MaxHeightInTopAttack + _deltaHeight;
                }
                else
                if (_targetHeight > MaxHeightInTopAttack)
                {
                    _doesAltitudeHolt = true;
                    _targetHeight = MaxHeightInTopAttack;
                }
                if (_debug) Debug.Log($"_doesAltitudeHolt: {_doesAltitudeHolt}");
                float turnAngle = 180f - (TopAttackLaunchAngle + TopAttackAttackAngle);
                float turnDistance = Mathf.PI * turnRadius * (180f - turnAngle) / 180f;
                float climbDistance = _targetHeight / Mathf.Sin(TopAttackLaunchAngle * Mathf.Deg2Rad);

                Vector2 highestPointInTriangle = new Vector2(Mathf.Cos(TopAttackLaunchAngle * Mathf.Deg2Rad) * climbDistance, Mathf.Sin(TopAttackLaunchAngle * Mathf.Deg2Rad) * climbDistance);
                float distanceToTurnCenter = turnRadius / Mathf.Sin((turnAngle / 2) * Mathf.Deg2Rad);

                Vector2 centerPoint = -highestPointInTriangle.normalized * distanceToTurnCenter;
                centerPoint.x = centerPoint.x * Mathf.Cos((turnAngle / 2) * Mathf.Deg2Rad) - centerPoint.y * Mathf.Sin((turnAngle / 2) * Mathf.Deg2Rad) + highestPointInTriangle.x;
                centerPoint.y = centerPoint.x * Mathf.Sin((turnAngle / 2) * Mathf.Deg2Rad) + centerPoint.y * Mathf.Cos((turnAngle / 2) * Mathf.Deg2Rad) + highestPointInTriangle.y;

                Vector2 tangentPointEnter =  highestPointInTriangle - highestPointInTriangle.normalized * distanceToTurnCenter;
                Vector2 tangentPointExit = highestPointInTriangle + Vector2.right * distanceToTurnCenter;

                _targetHeight = tangentPointEnter.y;

                if (_debug)
                {
                    Debug.Log($"TargetDir: {TargetPos - Projectile.transform.position}");
                    Debug.Log($"_deltaHeight: {_deltaHeight}");
                    Debug.Log($"_deltaXZ: {_deltaXZ}");
                    Debug.Log($"_heightCorrection: {_heightCorrection}");
                    Debug.Log($"_startingDistanceToTarget: {_startingDistanceToTarget}");
                    Debug.Log($"turnDistance: {turnDistance}");
                    Debug.Log($"climbDistance: {climbDistance}");
                    Debug.Log($"highestPointInTriangle: {highestPointInTriangle}");
                    Debug.Log($"distanceToTurnCenter: {distanceToTurnCenter}");
                    Debug.Log($"Final _targetHeight: {_targetHeight}");
                    Debug.Log($"tangentPointEnter: {tangentPointEnter}");
                }

                if (_doesAltitudeHolt)
                {
                    _altitudeHoldCurrentDistance = -turnDistance;
                    float descentDistance;
                    if (_deltaHeight > 0) descentDistance = (MaxHeightInTopAttack + _deltaHeight) / Mathf.Sin(TopAttackAttackAngle * Mathf.Deg2Rad);
                    else descentDistance = MaxHeightInTopAttack / Mathf.Sin(TopAttackAttackAngle * Mathf.Deg2Rad);
                    turnAngle = 180 - TopAttackAttackAngle;

                    highestPointInTriangle = new Vector2(_deltaXZ, 0) + new Vector2(- Mathf.Cos(TopAttackAttackAngle * Mathf.Deg2Rad) * descentDistance, Mathf.Sin(TopAttackAttackAngle * Mathf.Deg2Rad) * descentDistance);
                    distanceToTurnCenter = turnRadius / Mathf.Sin((turnAngle / 2) * Mathf.Deg2Rad);

                    Vector2 tangentPointEnter2 = highestPointInTriangle + Vector2.left * distanceToTurnCenter;

                    _altitudeHoldMaxDistance = Vector2.Distance(tangentPointEnter2, tangentPointExit);

                    if (_debug)
                    {
                        Debug.Log($"descentDistance: {descentDistance}");
                        Debug.Log($"highestPointInTriangle AltitudeHold: {highestPointInTriangle}");
                        Debug.Log($"tangentPointExit: {tangentPointExit}");
                        Debug.Log($"tangentPointEnter2: {tangentPointEnter2}");
                        Debug.Log($"_startingDistanceToTarget: {_startingDistanceToTarget}");
                        Debug.Log($"_altitudeHoldMaxDistance: {_altitudeHoldMaxDistance}");
                    }
                }


            }
            else
            {
                float _heightCorrection = _deltaHeight / Mathf.Tan(DirectAttackAttackAngle * Mathf.Deg2Rad);
                _startingDistanceToTarget = _deltaXZ + _heightCorrection;

                float deltaAngle = DirectAttackLaunchAngle - Vector3.Angle(Projectile.transform.forward, Vector3.ProjectOnPlane(Projectile.transform.forward, Vector3.up));

                _targetDirection = Quaternion.AngleAxis(-deltaAngle, Projectile.transform.right) * Projectile.transform.forward;
                _targetHeight = _startingDistanceToTarget * Mathf.Tan(DirectAttackLaunchAngle * Mathf.Deg2Rad) * Mathf.Tan(DirectAttackAttackAngle * Mathf.Deg2Rad) / (Mathf.Tan(DirectAttackLaunchAngle * Mathf.Deg2Rad) + Mathf.Tan(DirectAttackAttackAngle * Mathf.Deg2Rad));

                float turnAngle = 180f - (DirectAttackLaunchAngle + DirectAttackAttackAngle);
                float climbDistance = _targetHeight / Mathf.Sin(DirectAttackLaunchAngle * Mathf.Deg2Rad);

                Vector2 vertexPoint = new Vector2(Mathf.Cos(DirectAttackLaunchAngle * Mathf.Deg2Rad) * climbDistance, Mathf.Sin(DirectAttackLaunchAngle * Mathf.Deg2Rad) * climbDistance);
                float vertexDistance = turnRadius / Mathf.Sin(turnAngle * Mathf.Deg2Rad / 2);

                Vector2 centerPoint = -vertexPoint.normalized * vertexDistance;
                centerPoint.x = centerPoint.x * Mathf.Cos(turnAngle * Mathf.Deg2Rad / 2) - centerPoint.y * Mathf.Sin(turnAngle * Mathf.Deg2Rad / 2) + vertexPoint.x;
                centerPoint.y = centerPoint.x * Mathf.Sin(turnAngle * Mathf.Deg2Rad / 2) + centerPoint.y * Mathf.Cos(turnAngle * Mathf.Deg2Rad / 2) + vertexPoint.y;

                Vector2 tangentPoint = vertexPoint - vertexPoint.normalized * vertexDistance;

                _targetHeight = tangentPoint.y;
            }
        }

        public void Update()
        {
            if (AttackMode == EAttackMode.Top)
            {
                if (_doesAltitudeHolt && _attackState == AttackState.ClimbOut && Projectile.transform.position.y >= _targetHeight + _startingHeight)
                {
                    _attackState = AttackState.AltitudeHold;

                    _targetDirection = Quaternion.AngleAxis(TopAttackLaunchAngle, Projectile.transform.right) * Projectile.transform.forward;
                }
                else if (!_doesAltitudeHolt && _attackState == AttackState.ClimbOut && Projectile.transform.position.y >= _targetHeight + _startingHeight)
                {
                    _attackState = AttackState.Terminal;
                }

                if (_attackState == AttackState.AltitudeHold)
                {
                    if (_altitudeHoldCurrentDistance >= _altitudeHoldMaxDistance) _attackState = AttackState.Terminal;
                }
            }
            else
            {
                if (_attackState == AttackState.ClimbOut && Projectile.transform.position.y >= _targetHeight + _startingHeight)
                {
                    _attackState = AttackState.Terminal;
                }
            }
        }

		public void FixedUpdate()
        {
            Vector3 m_velocity = Projectile.m_velocity;
            Quaternion flightRotation = Quaternion.LookRotation(Projectile.transform.forward);

            Vector3 TargetPos = Vector3.zero;

            if (TargetRB != null) TargetPos = TargetRB.position;
            else if (TargetPoint != null) TargetPos = TargetPoint.Value;
            else Destroy(this);

            if (AttackMode == EAttackMode.Top)
            {
                if (_attackState == AttackState.ClimbOut || _attackState == AttackState.AltitudeHold)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(_targetDirection);
                    Quaternion deltaRotation = Quaternion.RotateTowards(flightRotation, targetRotation, TurningSpeed * Time.fixedDeltaTime) * Quaternion.Inverse(flightRotation);
                    Projectile.m_velocity = deltaRotation * m_velocity;
                }
                else if (_attackState == AttackState.Terminal)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(TargetPos - Projectile.transform.position);
                    Quaternion deltaRotation = Quaternion.RotateTowards(flightRotation, targetRotation, TurningSpeed * Time.fixedDeltaTime) * Quaternion.Inverse(flightRotation); 
                    Projectile.m_velocity = deltaRotation * m_velocity;
                }
            }
            else
            {
                if (_attackState == AttackState.ClimbOut)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(_targetDirection);
                    Quaternion deltaRotation = Quaternion.RotateTowards(flightRotation, targetRotation, TurningSpeed * Time.fixedDeltaTime) * Quaternion.Inverse(flightRotation);
                    Projectile.m_velocity = deltaRotation * m_velocity;
                }
                else if (_attackState == AttackState.Terminal)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(TargetPos - Projectile.transform.position);
                    Quaternion deltaRotation = Quaternion.RotateTowards(flightRotation, targetRotation, TurningSpeed * Time.fixedDeltaTime) * Quaternion.Inverse(flightRotation);
                    Projectile.m_velocity = deltaRotation * m_velocity;
                }
            }
            

            if (_attackState == AttackState.AltitudeHold)
            {
                _altitudeHoldCurrentDistance += m_velocity.magnitude * Time.fixedDeltaTime;
            }
        }
#endif
	}
}
