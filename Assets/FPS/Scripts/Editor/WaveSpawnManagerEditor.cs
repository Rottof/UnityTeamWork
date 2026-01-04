using UnityEditor;
using UnityEngine;
using Unity.FPS.AI;

namespace Unity.FPS.EditorExt
{
    [CustomEditor(typeof(WaveSpawnManager))]
    public class WaveSpawnManagerEditor : UnityEditor.Editor
    {
        private WaveSpawnManager manager;

        private void OnEnable()
        {
            manager = (WaveSpawnManager)target;
        }

        public override void OnInspectorGUI()
        {
            // 绘制默认Inspector
            DrawDefaultInspector();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("=== 快捷操作 ===", EditorStyles.boldLabel);

            // 运行时控制
            if (Application.isPlaying)
            {
                EditorGUILayout.HelpBox("游戏运行中 - 可以使用以下调试功能", MessageType.Info);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("🔄 手动刷新一波", GUILayout.Height(30)))
                {
                    manager.ManualSpawnWave();
                }
                if (GUILayout.Button("🔁 重置波次", GUILayout.Height(30)))
                {
                    manager.ResetWaves();
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.HelpBox("运行游戏后可使用调试功能", MessageType.Warning);
            }

            EditorGUILayout.Space(10);

            // 设置验证
            EditorGUILayout.LabelField("=== 设置验证 ===", EditorStyles.boldLabel);
            ValidateSetup();

            EditorGUILayout.Space(10);

            // 快速设置工具
            EditorGUILayout.LabelField("=== 快速设置工具 ===", EditorStyles.boldLabel);
            
            if (GUILayout.Button("🎯 自动查找玩家", GUILayout.Height(25)))
            {
                AutoFindPlayer();
            }

            if (GUILayout.Button("📍 创建大型怪刷新点", GUILayout.Height(25)))
            {
                CreateLargeEnemySpawnPoint();
            }

            if (GUILayout.Button("🔍 自动查找所有刷新点", GUILayout.Height(25)))
            {
                AutoFindSpawnPoints();
            }

            EditorGUILayout.Space(5);

            // 预设配置
            EditorGUILayout.LabelField("=== 预设配置 ===", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("简单难度"))
            {
                ApplyEasyPreset();
            }
            if (GUILayout.Button("普通难度"))
            {
                ApplyNormalPreset();
            }
            if (GUILayout.Button("困难难度"))
            {
                ApplyHardPreset();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void ValidateSetup()
        {
            int errorCount = 0;
            int warningCount = 0;

            // 检查玩家
            if (manager.player == null)
            {
                EditorGUILayout.HelpBox("❌ 未设置玩家引用", MessageType.Error);
                errorCount++;
            }
            else
            {
                EditorGUILayout.HelpBox("✓ 玩家引用已设置", MessageType.Info);
            }

            // 检查小型怪预制体
            if (manager.smallEnemyPrefab == null)
            {
                EditorGUILayout.HelpBox("❌ 未设置小型怪预制体", MessageType.Error);
                errorCount++;
            }
            else
            {
                // 验证预制体组件
                if (!ValidateEnemyPrefab(manager.smallEnemyPrefab))
                {
                    EditorGUILayout.HelpBox("⚠️ 小型怪预制体缺少必要组件", MessageType.Warning);
                    warningCount++;
                }
                else
                {
                    EditorGUILayout.HelpBox("✓ 小型怪预制体已正确设置", MessageType.Info);
                }
            }

            // 检查大型怪预制体
            if (manager.largeEnemyPrefab == null)
            {
                EditorGUILayout.HelpBox("⚠️ 未设置大型怪预制体（可选）", MessageType.Warning);
                warningCount++;
            }
            else
            {
                if (!ValidateEnemyPrefab(manager.largeEnemyPrefab))
                {
                    EditorGUILayout.HelpBox("⚠️ 大型怪预制体缺少必要组件", MessageType.Warning);
                    warningCount++;
                }
                else
                {
                    EditorGUILayout.HelpBox("✓ 大型怪预制体已正确设置", MessageType.Info);
                }
            }

            // 检查刷新点
            if (manager.largeEnemySpawnPoints.Count == 0)
            {
                EditorGUILayout.HelpBox("⚠️ 未设置大型怪刷新点（可选）", MessageType.Warning);
                warningCount++;
            }
            else
            {
                EditorGUILayout.HelpBox($"✓ 已设置 {manager.largeEnemySpawnPoints.Count} 个大型怪刷新点", MessageType.Info);
            }

            // 检查EnemyManager
            if (FindObjectOfType<EnemyManager>() == null)
            {
                EditorGUILayout.HelpBox("❌ 场景中找不到 EnemyManager", MessageType.Error);
                errorCount++;
            }
            else
            {
                EditorGUILayout.HelpBox("✓ EnemyManager 已存在", MessageType.Info);
            }

            // 总结
            EditorGUILayout.Space(5);
            if (errorCount == 0 && warningCount == 0)
            {
                EditorGUILayout.HelpBox("✅ 所有设置正确！可以开始测试。", MessageType.Info);
            }
            else if (errorCount > 0)
            {
                EditorGUILayout.HelpBox($"⚠️ 发现 {errorCount} 个错误，{warningCount} 个警告", MessageType.Error);
            }
            else
            {
                EditorGUILayout.HelpBox($"⚠️ 发现 {warningCount} 个警告", MessageType.Warning);
            }
        }

        private bool ValidateEnemyPrefab(GameObject prefab)
        {
            if (prefab == null) return false;

            bool hasHealth = prefab.GetComponent<Unity.FPS.Game.Health>() != null;
            bool hasEnemyController = prefab.GetComponent<EnemyController>() != null;
            
            return hasHealth && hasEnemyController;
        }

        private void AutoFindPlayer()
        {
            var playerController = FindObjectOfType<Unity.FPS.Gameplay.PlayerCharacterController>();
            if (playerController != null)
            {
                Undo.RecordObject(manager, "Auto Find Player");
                manager.player = playerController.transform;
                EditorUtility.SetDirty(manager);
                Debug.Log("✓ 已自动找到玩家引用");
            }
            else
            {
                Debug.LogWarning("❌ 场景中找不到 PlayerCharacterController");
            }
        }

        private void CreateLargeEnemySpawnPoint()
        {
            GameObject spawnPoint = new GameObject("LargeEnemySpawnPoint");
            spawnPoint.transform.position = SceneView.lastActiveSceneView.camera.transform.position + 
                                           SceneView.lastActiveSceneView.camera.transform.forward * 5f;
            
            var component = spawnPoint.AddComponent<LargeEnemySpawnPoint>();
            
            Undo.RegisterCreatedObjectUndo(spawnPoint, "Create Large Enemy Spawn Point");
            
            // 自动添加到列表
            if (!manager.largeEnemySpawnPoints.Contains(component))
            {
                Undo.RecordObject(manager, "Add Spawn Point to List");
                manager.largeEnemySpawnPoints.Add(component);
                EditorUtility.SetDirty(manager);
            }
            
            Selection.activeGameObject = spawnPoint;
            Debug.Log("✓ 已创建大型怪刷新点");
        }

        private void AutoFindSpawnPoints()
        {
            var spawnPoints = FindObjectsOfType<LargeEnemySpawnPoint>();
            
            Undo.RecordObject(manager, "Auto Find Spawn Points");
            manager.largeEnemySpawnPoints.Clear();
            
            foreach (var point in spawnPoints)
            {
                if (point.isActive)
                {
                    manager.largeEnemySpawnPoints.Add(point);
                }
            }
            
            EditorUtility.SetDirty(manager);
            Debug.Log($"✓ 已找到并添加 {manager.largeEnemySpawnPoints.Count} 个刷新点");
        }

        private void ApplyEasyPreset()
        {
            Undo.RecordObject(manager, "Apply Easy Preset");
            manager.waveInterval = 40f;
            manager.initialSmallEnemyLimit = 3;
            manager.limitIncreasePerWave = 1;
            manager.healthIncreasePerWave = 0.05f;
            manager.damageIncreasePerWave = 0.05f;
            EditorUtility.SetDirty(manager);
            Debug.Log("✓ 已应用简单难度预设");
        }

        private void ApplyNormalPreset()
        {
            Undo.RecordObject(manager, "Apply Normal Preset");
            manager.waveInterval = 30f;
            manager.initialSmallEnemyLimit = 5;
            manager.limitIncreasePerWave = 2;
            manager.healthIncreasePerWave = 0.1f;
            manager.damageIncreasePerWave = 0.1f;
            EditorUtility.SetDirty(manager);
            Debug.Log("✓ 已应用普通难度预设");
        }

        private void ApplyHardPreset()
        {
            Undo.RecordObject(manager, "Apply Hard Preset");
            manager.waveInterval = 20f;
            manager.initialSmallEnemyLimit = 7;
            manager.limitIncreasePerWave = 3;
            manager.healthIncreasePerWave = 0.15f;
            manager.damageIncreasePerWave = 0.15f;
            EditorUtility.SetDirty(manager);
            Debug.Log("✓ 已应用困难难度预设");
        }

        // 在Scene视图中绘制辅助线
        private void OnSceneGUI()
        {
            if (manager.player != null)
            {
                // 绘制刷新范围
                Handles.color = new Color(1, 1, 0, 0.1f);
                Handles.DrawSolidDisc(manager.player.position, Vector3.up, manager.spawnRadius);
                
                Handles.color = Color.yellow;
                Handles.DrawWireDisc(manager.player.position, Vector3.up, manager.spawnRadius);
                
                // 添加文本标签
                Handles.Label(manager.player.position + Vector3.up * 2f, 
                    $"小型怪刷新范围\n半径: {manager.spawnRadius}m",
                    new GUIStyle()
                    {
                        normal = new GUIStyleState() { textColor = Color.yellow },
                        fontSize = 12,
                        fontStyle = FontStyle.Bold
                    });
            }
        }
    }
}

