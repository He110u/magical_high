using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Unity.Animations.SpringBones
{
    public class SpringBoneWindow : EditorWindow
    {
        [MenuItem("UTJ/스프링 본 창")]
        public static void ShowWindow()
        {
            var window = GetWindow<SpringBoneWindow>("스프링 본");
            window.OnShow();
        }

        private GUIElements.Column mainUI;
        private Vector2 scrollPosition;

        private Texture headerIcon;
        private Texture newDocumentIcon;
        private Texture openDocumentIcon;
        private Texture saveDocumentIcon;
        private Texture deleteIcon;
        private Texture pivotIcon;
        private Texture sphereIcon;
        private Texture capsuleIcon;
        private Texture panelIcon;

        private SpringBoneSettings settings;

        private static Texture LoadIcon(string iconDirectory, string filename)
        {
            var iconPath = PathUtil.CombinePath(iconDirectory, filename);
            var iconTexture = AssetDatabase.LoadAssetAtPath<Texture>(iconPath);
            if (iconTexture == null)
            {
                Debug.LogWarning("아이콘 로드 실패:\n" + iconPath);
            }
            return iconTexture;
        }

        private static string FindIconAssetDirectory()
        {
            return DirectoryUtil.GetFilesRecursively(Application.dataPath, "SpringCapsuleIcon.tga")
                .Select(path => PathUtil.NormalizePath(path))
                .Where(path => path.ToLowerInvariant().Contains("editor/springbone/gui/icons/"))
                .Select(path => PathUtil.SystemPathToAssetPath(System.IO.Path.GetDirectoryName(path)))
                .FirstOrDefault();
        }

        private void InitializeIcons()
        {
            if (headerIcon != null) { return; }

            var iconDirectory = FindIconAssetDirectory();
            if (iconDirectory == null)
            {
                Debug.LogWarning("SpringBoneWindow의 아이콘 디렉토리를 찾을 수 없습니다");
                return;
            }

            headerIcon = LoadIcon(iconDirectory, "SpringIcon.tga");
            newDocumentIcon = LoadIcon(iconDirectory, "NewDocumentHS.png");
            openDocumentIcon = LoadIcon(iconDirectory, "OpenHH.bmp");
            saveDocumentIcon = LoadIcon(iconDirectory, "SaveHH.bmp");
            deleteIcon = LoadIcon(iconDirectory, "Delete.png");
            pivotIcon = LoadIcon(iconDirectory, "Pivot.png");
            sphereIcon = LoadIcon(iconDirectory, "SpringSphereIcon.tga");
            capsuleIcon = LoadIcon(iconDirectory, "SpringCapsuleIcon.tga");
            panelIcon = LoadIcon(iconDirectory, "SpringPanelIcon.tga");
        }

        private void InitializeButtonGroups()
        {
            if (mainUI != null) { return; }

            const float BigButtonHeight = 60f;

            System.Func<GUIStyle> headerLabelStyleProvider = () => SpringBoneGUIStyles.HeaderLabelStyle;
            System.Func<GUIStyle> buttonLabelStyleProvider = () => SpringBoneGUIStyles.MiddleLeftJustifiedLabelStyle;

            mainUI = new GUIElements.Column(new GUIElements.IElement[]
            {
                new GUIElements.Column(new GUIElements.IElement[]
                {
                    new GUIElements.Label("다이내믹스 CSV", headerLabelStyleProvider),
                    new GUIElements.Row(new GUIElements.IElement[]
                    {
                        new GUIElements.Button("불러오기", LoadSpringBoneSetupWindow.ShowWindow, openDocumentIcon, buttonLabelStyleProvider),
                        new GUIElements.Button("저장", SaveSpringBoneSetupWindow.ShowWindow, saveDocumentIcon, buttonLabelStyleProvider)
                    },
                    BigButtonHeight)
                }),

                new GUIElements.Column(new GUIElements.IElement[]
                {
                    new GUIElements.Label("스프링 본", headerLabelStyleProvider),
                    new GUIElements.Row(new GUIElements.IElement[]
                    {
                        new GUIElements.Button("스프링\n본 추가", SpringBoneEditorActions.AssignSpringBonesRecursively, headerIcon, buttonLabelStyleProvider),
                        new GUIElements.Button("피벗 생성", SpringBoneEditorActions.CreatePivotForSpringBones, pivotIcon, buttonLabelStyleProvider)
                    },
                    BigButtonHeight),
                    new GUIElements.Button("매니저 생성/업데이트", SpringBoneEditorActions.AddToOrUpdateSpringManagerInSelection, newDocumentIcon, buttonLabelStyleProvider),
                    new GUIElements.Separator(),
                    new GUIElements.Button("스프링 본 미러", MirrorSpringBoneWindow.ShowWindow, null, buttonLabelStyleProvider),
                    new GUIElements.Button("선택 및 하위 스프링 본 선택", SpringBoneEditorActions.SelectChildSpringBones, null, buttonLabelStyleProvider),
                    new GUIElements.Button("선택한 스프링 본 삭제", SpringBoneEditorActions.DeleteSelectedBones, deleteIcon, buttonLabelStyleProvider),
                    new GUIElements.Button("선택 및 하위 매니저/본 삭제", SpringBoneEditorActions.DeleteSpringBonesAndManagers, deleteIcon, buttonLabelStyleProvider),
                }),

                new GUIElements.Column(new GUIElements.IElement[]
                {
                    new GUIElements.Label("콜리전", headerLabelStyleProvider),
                    new GUIElements.Row(new GUIElements.IElement[]
                    {
                        new GUIElements.Button("구체", SpringColliderEditorActions.CreateSphereColliderBeneathSelectedObjects, sphereIcon, buttonLabelStyleProvider),
                        new GUIElements.Button("캡슐", SpringColliderEditorActions.CreateCapsuleColliderBeneathSelectedObjects, capsuleIcon, buttonLabelStyleProvider),
                        new GUIElements.Button("패널", SpringColliderEditorActions.CreatePanelColliderBeneathSelectedObjects, panelIcon, buttonLabelStyleProvider),
                    },
                    BigButtonHeight),
                    new GUIElements.Button("캡슐 위치를 부모에 맞추기", SpringColliderEditorActions.AlignSelectedCapsulesToParents, capsuleIcon, buttonLabelStyleProvider),
                    new GUIElements.Button("스프링 본에서 콜리전 제거", SpringColliderEditorActions.DeleteCollidersFromSelectedSpringBones, deleteIcon, buttonLabelStyleProvider),
                    new GUIElements.Button("선택 및 하위 콜라이더 삭제", SpringColliderEditorActions.DeleteAllChildCollidersFromSelection, deleteIcon, buttonLabelStyleProvider),
                    new GUIElements.Button("정리 (Cleanup)", SpringColliderEditorActions.CleanUpDynamics, deleteIcon, buttonLabelStyleProvider)
                })
            },
            false,
            0f);
        }

        private Rect GetScrollContentsRect()
        {
            const int ScrollbarWidth = 24;
            var width = position.width - GUIElements.Spacing - ScrollbarWidth;
            var height = mainUI.Height;
            return new Rect(0f, 0f, width, height);
        }

        private void OnGUI()
        {
            if (settings == null) { LoadSettings(); }

            SpringBoneGUIStyles.ReacquireStyles();
            InitializeIcons();
            InitializeButtonGroups();

            var xPos = GUIElements.Spacing;
            var yPos = GUIElements.Spacing;
            var scrollContentsRect = GetScrollContentsRect();
            yPos = ShowHeaderUI(xPos, yPos, scrollContentsRect.width);
            var scrollViewRect = new Rect(0f, yPos, position.width, position.height - yPos);
            scrollPosition = GUI.BeginScrollView(scrollViewRect, scrollPosition, scrollContentsRect);
            mainUI.DoUI(GUIElements.Spacing, 0f, scrollContentsRect.width);
            GUI.EndScrollView();

            ApplySettings();
        }

        private float ShowHeaderUI(float xPos, float yPos, float uiWidth)
        {
            var needToRepaint = false;
            System.Func<GUIStyle> headerLabelStyleProvider = () => SpringBoneGUIStyles.HeaderLabelStyle;
            System.Func<GUIStyle> toggleStyleProvider = () => SpringBoneGUIStyles.ToggleStyle;

            var headerColumn = new GUIElements.Column(
                new GUIElements.IElement[] {
                    new GUIElements.Label("표시", headerLabelStyleProvider),
                    new GUIElements.Row(new GUIElements.IElement[]
                        {
                            new GUIElements.Toggle("선택한 본만 표시", () => settings.onlyShowSelectedBones, newValue => { settings.onlyShowSelectedBones = newValue; needToRepaint = true; }, toggleStyleProvider),
                            new GUIElements.Toggle("본 콜리전 표시", () => settings.showBoneSpheres, newValue => { settings.showBoneSpheres = newValue; needToRepaint = true; }, toggleStyleProvider),
                        },
                        GUIElements.RowHeight),
                    new GUIElements.Row(new GUIElements.IElement[]
                        {
                            new GUIElements.Toggle("선택한 콜라이더만 표시", () => settings.onlyShowSelectedColliders, newValue => { settings.onlyShowSelectedColliders = newValue; needToRepaint = true; }, toggleStyleProvider),
                            new GUIElements.Toggle("본 이름 표시", () => settings.showBoneNames, newValue => { settings.showBoneNames = newValue; needToRepaint = true; }, toggleStyleProvider)
                        },
                        GUIElements.RowHeight),
                },
                true, 4f, 0f);

            headerColumn.DoUI(xPos, yPos, uiWidth);

            if (needToRepaint)
            {
                ApplySettings();
                SaveSettings();
                SceneView.RepaintAll();
            }

            return yPos + headerColumn.Height + GUIElements.Spacing;
        }

        private void ApplySettings()
        {
            SpringManager.onlyShowSelectedBones = settings.onlyShowSelectedBones;
            SpringManager.showBoneSpheres = settings.showBoneSpheres;
            SpringManager.onlyShowSelectedColliders = settings.onlyShowSelectedColliders;
            SpringManager.showBoneNames = settings.showBoneNames;
        }

        private void LoadSettings()
        {
            if (settings == null)
            {
                settings = SpringBoneSettings.GetDefaultSettings();
            }
        }

        private void SaveSettings() { }

        private void OnShow()
        {
            LoadSettings();
        }

        [System.Serializable]
        private class SpringBoneSettings
        {
            public bool onlyShowSelectedBones;
            public bool onlyShowSelectedColliders;
            public bool showBoneSpheres;
            public bool showBoneNames;

            public static SpringBoneSettings GetDefaultSettings()
            {
                return new SpringBoneSettings
                {
                    onlyShowSelectedBones = true,
                    onlyShowSelectedColliders = true,
                    showBoneSpheres = true,
                    showBoneNames = false
                };
            }
        }
    }
}