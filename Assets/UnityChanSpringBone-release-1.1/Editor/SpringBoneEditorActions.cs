using Unity.Animations.SpringBones.GameObjectExtensions;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Unity.Animations.SpringBones
{
    public static class SpringBoneEditorActions
    {
        public static void ShowSpringBoneWindow()
        {
            SpringBoneWindow.ShowWindow();
        }

        public static void AssignSpringBonesRecursively()
        {
            if (Application.isPlaying)
            {
                Debug.LogError("플레이 모드를 종료해주세요.");
                return;
            }

            if (Selection.gameObjects.Length < 1)
            {
                Debug.LogError("하나 이상의 오브젝트를 선택해주세요.");
                return;
            }

            var springManagers = new HashSet<SpringManager>();
            foreach (var gameObject in Selection.gameObjects)
            {
                SpringBoneSetup.AssignSpringBonesRecursively(gameObject.transform);
                var manager = gameObject.GetComponentInParent<SpringManager>();
                if (manager != null)
                {
                    springManagers.Add(manager);
                }
            }

            foreach (var manager in springManagers)
            {
                SpringBoneSetup.FindAndAssignSpringBones(manager, true);
            }

            AssetDatabase.Refresh();
        }

        public static void CreatePivotForSpringBones()
        {
            if (Application.isPlaying)
            {
                Debug.LogError("플레이 모드를 종료해주세요.");
                return;
            }

            if (Selection.gameObjects.Length < 1)
            {
                Debug.LogError("하나 이상의 오브젝트를 선택해주세요.");
                return;
            }

            var selectedSpringBones = Selection.gameObjects
                .Select(gameObject => gameObject.GetComponent<SpringBone>())
                .Where(bone => bone != null);

            foreach (var springBone in selectedSpringBones)
            {
                SpringBoneSetup.CreateSpringPivotNode(springBone);
            }
        }

        public static void AddToOrUpdateSpringManagerInSelection()
        {
            if (Application.isPlaying)
            {
                Debug.LogError("플레이 모드를 종료해주세요.");
                return;
            }

            if (Selection.gameObjects.Length <= 0)
            {
                Debug.LogError("하나 이상의 오브젝트를 선택해주세요.");
                return;
            }

            foreach (var gameObject in Selection.gameObjects)
            {
                var manager = gameObject.GetComponent<SpringManager>();
                if (manager == null)
                {
                    manager = gameObject.AddComponent<SpringManager>();
                }

                SpringBoneSetup.FindAndAssignSpringBones(manager, true);
            }
        }

        public static void SelectChildSpringBones()
        {
            var springBoneObjects = Selection.gameObjects
                .SelectMany(gameObject => gameObject.GetComponentsInChildren<SpringBone>(true))
                .Select(bone => bone.gameObject)
                .Distinct()
                .ToArray();

            Selection.objects = springBoneObjects;
        }

        public static void DeleteSpringBonesAndManagers()
        {
            if (Application.isPlaying)
            {
                Debug.LogError("플레이 모드를 종료해주세요.");
                return;
            }

            if (Selection.gameObjects.Length != 1)
            {
                Debug.LogError("하나의 루트 오브젝트만 선택해주세요.");
                return;
            }

            var rootObject = Selection.gameObjects.First();

            var queryMessage =
                "정말로 이 오브젝트와 그 하위의 모든\n" +
                "스프링 본과 스프링 매니저를 삭제하시겠습니까?\n\n" +
                rootObject.name;

            if (EditorUtility.DisplayDialog(
                "스프링 본 및 매니저 삭제",
                queryMessage,
                "삭제",
                "취소"))
            {
                SpringBoneSetup.DestroySpringManagersAndBones(rootObject);
                AssetDatabase.Refresh();
            }
        }

        public static void DeleteSelectedBones()
        {
            var springBonesToDelete = GameObjectUtil.FindComponentsOfType<SpringBone>()
                .Where(bone => Selection.gameObjects.Contains(bone.gameObject))
                .ToArray();

            var springManagersToUpdate = GameObjectUtil.FindComponentsOfType<SpringManager>()
                .Where(manager => manager.springBones.Any(bone => springBonesToDelete.Contains(bone)))
                .ToArray();

            Undo.RecordObjects(springManagersToUpdate, "선택된 본 삭제");

            foreach (var boneToDelete in springBonesToDelete)
            {
                Undo.DestroyObjectImmediate(boneToDelete);
            }

            foreach (var manager in springManagersToUpdate)
            {
                manager.FindSpringBones(true);
            }
        }

        public static void PromptToUpdateSpringBonesFromList()
        {
            if (Application.isPlaying)
            {
                Debug.LogError("플레이 중에는 업데이트할 수 없습니다.");
                return;
            }

            var selectedSpringManagers = Selection.gameObjects
                .Select(gameObject => gameObject.GetComponent<SpringManager>())
                .Where(manager => manager != null)
                .ToArray();

            if (!selectedSpringManagers.Any())
            {
                selectedSpringManagers = GameObjectUtil.FindComponentsOfType<SpringManager>().ToArray();
            }

            if (selectedSpringManagers.Count() != 1)
            {
                Debug.LogError("SpringManager 하나만 선택해주세요.");
                return;
            }

            var springManager = selectedSpringManagers.First();

            var queryMessage =
                "본 리스트를 기준으로 흔들림 본을 업데이트하시겠습니까?\n\n" +
                "리스트에 없는 SpringBone은 삭제되고,\n" +
                "모델에 존재하지만 리스트에 없는 SpringBone은 추가됩니다.\n\n" +
                "SpringManager: " + springManager.name;

            if (EditorUtility.DisplayDialog(
                "본 리스트 기준 업데이트",
                queryMessage,
                "업데이트",
                "취소"))
            {
                AutoSpringBoneSetup.UpdateSpringManagerFromBoneList(springManager);
            }
        }
    }
}