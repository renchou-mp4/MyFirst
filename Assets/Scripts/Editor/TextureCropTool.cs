using System.IO;
using UnityEditor;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace yxy
{
    public class TextureCropTool : EditorWindow
    {
        //配置参数
        private Texture2D _oldTexture;
        private Texture2D _newTexture;
        private Texture2D _lastTexture;

        private TextureImporter _importer;
        private const float _baseRect = 128;
        private float _alphaThreshold = 0.01f;
        private int _padding = 0;
        private string _assetPath = "";
        private Color _boundsColor = Color.green;
        private Rect _newRect;
        private Rect _oldRect;
        private Rect _newBounds;


        [MenuItem("Tools/图片裁剪")]
        public static void ShowWindow()
        {
            GetWindow<TextureCropTool>("图片裁剪");
        }

        private void OnGUI()
        {
            DrawSettings();
            DrawLayout();
        }

        private void DrawSettings()
        {
            EditorGUILayout.LabelField("图片裁剪设置", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            _alphaThreshold = EditorGUILayout.Slider("透明阈值", _alphaThreshold, 0f, 10f);
            _padding = EditorGUILayout.IntSlider("外边距", _padding, 0, 10);
            _boundsColor = EditorGUILayout.ColorField("包围盒颜色", _boundsColor);


            EditorGUILayout.Space();
            EditorGUILayout.LabelField("资源路径", _assetPath);
        }

        private void DrawLayout()
        {
            _oldTexture = _newTexture = EditorGUILayout.ObjectField("选择图片：", _oldTexture, typeof(Texture2D), false) as Texture2D;
            if (_lastTexture == null || _lastTexture != _oldTexture)
            {
                _assetPath = AssetDatabase.GetAssetPath(_oldTexture);
                _lastTexture = _oldTexture;
                _newBounds = Rect.zero;
            }
            if (_oldTexture == null) return;

            EditorGUILayout.Separator();
            GUILayout.Space(10);
            GUILayout.BeginVertical();

            //原图宽高比
            float whRatio = _oldTexture.width * 1.0f / _oldTexture.height;

            //原图
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (_oldTexture is not null)
            {
                EditorGUILayout.LabelField("原图：", GUILayout.MaxWidth(100));
                _oldRect = GUILayoutUtility.GetRect(_baseRect * whRatio, _baseRect, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
                GUI.DrawTexture(_oldRect, _oldTexture);
                GUILayout.BeginVertical();
                EditorGUILayout.LabelField("显示大小：", $"{_oldRect.width} x {_oldRect.height}", GUILayout.MaxWidth(300));
                EditorGUILayout.LabelField("图标大小：", $"{_oldTexture.width} x {_oldTexture.height}", GUILayout.MaxWidth(300));
                GUILayout.EndVertical();
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(10);

            //预览图
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (_newTexture is not null)
            {
                EditorGUILayout.LabelField("预览图：", GUILayout.MaxWidth(100));
                _newRect = GUILayoutUtility.GetRect(_baseRect * whRatio, _baseRect, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
                GUI.DrawTexture(_newRect, _newTexture);
                GUILayout.BeginVertical();
                EditorGUILayout.LabelField("显示大小：", $"{_newRect.width} x {_newRect.height}", GUILayout.MaxWidth(300));
                EditorGUILayout.LabelField("图标大小：", $"{_newTexture.width} x {_newTexture.height}", GUILayout.MaxWidth(300));
                GUILayout.EndVertical();
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();


            //计算包围盒
            Handles.BeginGUI();
            Handles.color = _boundsColor;
            if (GUILayout.Button("计算包围盒", GUILayout.Height(30)))
            {
                _newBounds = CalculateNewBounds();
            }
            if (_oldTexture != null && _newTexture != null)
            {
                Handles.DrawWireCube(_oldRect.center, new Vector2(_oldRect.width, _oldRect.height));
                Handles.DrawWireCube(_newRect.center, new Vector2(_newBounds.width * _oldRect.width / _oldTexture.width, _newBounds.height * _oldRect.height / _oldTexture.height));
            }

            if (GUILayout.Button("裁剪", GUILayout.Height(60)))
            {
                CropTexture();
            }
            Handles.EndGUI();

            GUILayout.EndVertical();
        }

        private Rect CalculateNewBounds()
        {
            if (_newTexture == null) return Rect.zero;

            _importer = AssetImporter.GetAtPath(_assetPath) as TextureImporter;

            Texture2D readableTex = GetReadableTexture();
            if (readableTex == null) return Rect.zero;

            Color32[] pixels = readableTex.GetPixels32();
            int width = readableTex.width;
            int height = readableTex.height;

            int minX = width;
            int minY = height;
            int maxX = 0;
            int maxY = 0;

            //查找非透明像素边界
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color32 pixel = pixels[y * width + x];
                    if (pixel.a > _alphaThreshold * 255) //非透明像素
                    {
                        if (x < minX) minX = x;
                        if (x > maxX) maxX = x;
                        if (y < minY) minY = y;
                        if (y > maxY) maxY = y;
                    }
                }
            }

            //没有非透明像素，返回整个纹理
            if (minX > maxX || minY > maxY)
                return new Rect(0, 0, width, height);

            //添加安全边界
            minX = Mathf.Max(0, minX - 1);
            minY = Mathf.Max(0, minY - 1);
            maxX = Mathf.Min(width - 1, maxX + 1);
            maxY = Mathf.Min(height - 1, maxY + 1);

            int cropWidth = maxX - minX + 1;
            int cropHeight = maxY - minY + 1;

            return new Rect(minX, minY, cropWidth, cropHeight);
        }

        private void CropTexture()
        {
            if (_newBounds.width <= 0 || _newBounds.height <= 0)
            {
                Log.Error($"裁剪失败！裁剪包围盒非法：{_newBounds.width} x {_newBounds.height}");
                return;
            }

            //创建新纹理
            Texture2D croppedTexture = new Texture2D(
                (int)_newBounds.width + _padding * 2,
                (int)_newBounds.height + _padding * 2,
                TextureFormat.RGBA32,
                false);

            //填充透明背景
            Color[] pixels = new Color[croppedTexture.width * croppedTexture.height];

            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.clear;
            }
            croppedTexture.SetPixels(pixels);

            //复制有效区域
            Texture2D readableTex = GetReadableTexture();
            Color[] readablePixels = readableTex.GetPixels(
                (int)_newBounds.x,
                (int)_newBounds.y,
                (int)_newBounds.width,
                (int)_newBounds.height);

            croppedTexture.SetPixels(_padding, _padding, (int)_newBounds.width, (int)_newBounds.height, readablePixels);
            croppedTexture.Apply();

            //保存纹理
            string outputPath = _assetPath.GetDirectoryPath();
            string fileName = Path.GetFileNameWithoutExtension(_assetPath);
            string extension = Path.GetExtension(_assetPath);

            //确保输出目录存在
            if (!Directory.Exists(outputPath) && !outputPath.IsNullOrEmpty())
            {
                Directory.CreateDirectory(outputPath);
            }

            string path = $"{outputPath}/{fileName}_cropped{extension}";
            byte[] pngData = croppedTexture.EncodeToPNG();
            File.WriteAllBytes(path, pngData);

            //清理临时纹理
            DestroyImmediate(croppedTexture);
            if (readableTex != null)
                DestroyImmediate(readableTex);

            //导入设置
            AssetDatabase.Refresh();
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
            }

            Log.Info($"裁剪完成！保存路径：{path}");
        }

        private Texture2D GetReadableTexture()
        {
            //通过复制获得可读纹理
            RenderTexture readerTex = RenderTexture.GetTemporary(
                _newTexture.width,
                _newTexture.height,
                0,
                RenderTextureFormat.Default,
                RenderTextureReadWrite.Linear);

            Graphics.Blit(_newTexture, readerTex);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = readerTex;

            Texture2D readableTex = new Texture2D(_newTexture.width, _newTexture.height);
            readableTex.ReadPixels(new Rect(0, 0, readerTex.width, readerTex.height), 0, 0);
            readableTex.Apply();

            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(readerTex);

            return readableTex;
        }

    }
}

