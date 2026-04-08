using UnityEngine;
using UnityEditor;
using System.IO;

public class BuiltInAssetExporter : EditorWindow
{
    [MenuItem("Tools/Export Built-in Sprites (Fixed)")]
    public static void ExportSpritesFixed()
    {
        string path = EditorUtility.SaveFolderPanel("스프라이트를 저장할 폴더 선택", Application.dataPath, "");
        if (string.IsNullOrEmpty(path)) return;
        if (!path.EndsWith("/")) path += "/";

        Sprite[] sprites = Resources.FindObjectsOfTypeAll<Sprite>();
        int exportCount = 0;

        foreach (Sprite sprite in sprites)
        {
            if (sprite.name.Contains("UISprite") || sprite.name.Contains("Knob") ||
                sprite.name.Contains("Checkmark") || sprite.name.Contains("Background") ||
                sprite.name.Contains("Dropdown") || sprite.name.Contains("Mask"))
            {
                // 수정된 추출 함수 호출
                Texture2D extractedTex = ExtractSpriteToTexture(sprite);

                if (extractedTex != null)
                {
                    byte[] bytes = extractedTex.EncodeToPNG();
                    File.WriteAllBytes(path + sprite.name + ".png", bytes);
                    exportCount++;
                }
            }
        }
        AssetDatabase.Refresh();
        Debug.Log($"추출 완료! 총 {exportCount}개의 정상적인 스프라이트가 저장되었습니다.");
    }

    // 아틀라스에 묶인 스프라이트의 지정된 영역(Rect)만 캡처하여 가져오는 함수
    private static Texture2D ExtractSpriteToTexture(Sprite sprite)
    {
        if (sprite.rect.width == 0 || sprite.rect.height == 0) return null;

        // 1. 원본 텍스처 크기와 동일한 임시 렌더 텍스처 생성 (읽기/쓰기 우회용)
        RenderTexture tmp = RenderTexture.GetTemporary(
            sprite.texture.width,
            sprite.texture.height,
            0,
            RenderTextureFormat.Default,
            RenderTextureReadWrite.Linear
        );

        // 2. 원본 텍스처의 그래픽 데이터를 렌더 텍스처로 덮어씌우기 (Blit)
        Graphics.Blit(sprite.texture, tmp);

        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = tmp;

        // 3. 스프라이트의 실제 크기(Rect)만큼만 담을 깨끗한 Texture2D 생성
        Texture2D newTex = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height, TextureFormat.RGBA32, false);

        // 4. 렌더 텍스처에서 정확히 스프라이트 영역(Rect)의 좌표만큼만 잘라서 읽어오기
        newTex.ReadPixels(new Rect(sprite.rect.x, sprite.rect.y, sprite.rect.width, sprite.rect.height), 0, 0);
        newTex.Apply();

        // 5. 메모리 누수 방지
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(tmp);

        return newTex;
    }
}