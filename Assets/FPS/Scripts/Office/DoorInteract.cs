using System;
using Codice.Client.BaseCommands;
using UnityEngine;

namespace FPS.Scripts.Office
{
    public class DoorInteract : MonoBehaviour
    {
        public Transform doorModel;
        public float openAngle = 90f;
        public float speed = 3f;

        public bool isOpen;
        private Quaternion closeRot;
        private Quaternion openRot;

        void Start()
        {
            if (!isOpen)
            {
                closeRot = doorModel.localRotation;
                openRot = Quaternion.Euler(
                    doorModel.localEulerAngles + Vector3.up * openAngle
                );
            }
            else
            {
                openRot = doorModel.localRotation;
                closeRot = Quaternion.Euler(
                    doorModel.localEulerAngles - Vector3.up * openAngle
                );
            }
        }

        public void Toggle(Ray ray)
        {
            isOpen = !isOpen;

            if (isOpen)
            {
                // 射线方向的水平投影（忽略Y轴）
                Vector3 dir = ray.direction;
                dir.y = 0;

                if (dir.sqrMagnitude < 0.001f) return;

                dir.Normalize();

                // 计算门当前朝向与射线方向的夹角
                float angle = Vector3.SignedAngle(transform.right, dir, Vector3.up);
                //最大90度
                if (angle > 0)
                {
                    angle = openAngle;
                }
                else
                {
                    angle = -openAngle;
                }
                // 基于关闭旋转，绕Y轴旋转
                openRot = closeRot * Quaternion.Euler(0, angle, 0);
            }
        }


        void Update()
        {
            Quaternion target = isOpen ? openRot : closeRot;
            doorModel.localRotation = Quaternion.Slerp(
                doorModel.localRotation,
                target,
                Time.deltaTime * speed
            );
        }
    }
}