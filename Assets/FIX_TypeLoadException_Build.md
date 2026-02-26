# Xử lý TypeLoadException khi Build (OnSceneProcess)

Lỗi: `TypeLoadException: Failure has occurred while loading a type` tại `BuildPipelineInterfaces:OnSceneProcess`.

## Bước 1: Clean và reimport (làm trước)

1. **Đóng Unity hoàn toàn.**
2. Xóa thư mục **`Library`** trong project (không xóa thư mục `Assets`, `Packages`, `ProjectSettings`).
3. Mở lại project bằng Unity. Đợi Unity reimport xong (có thể mất vài phút).
4. Thử build lại.

## Bước 2: Nếu vẫn lỗi – tìm callback gây lỗi

Lỗi xảy ra khi Unity gọi các build callback (xử lý scene). Tạm thời tắt từng nhóm để khoanh vùng:

- **Build Report Tool**: đổi tên thư mục `Assets/BuildReport` → `Assets/BuildReport_OFF` (hoặc xóa/disable).
- **CodeStage Anti-Cheat (Injection Detector)**: trong Editor tắt Injection Detector trong settings của ACTk, hoặc tạm đổi tên thư mục `Assets/Base/3Party/CodeStage/AntiCheatToolkit`.

Sau mỗi lần tắt một nhóm → mở lại Unity → build lại. Nếu build qua được thì nhóm vừa tắt có liên quan lỗi.

## Bước 3: Giảm code stripping (build IL2CPP)

Nếu bạn build **Android/iOS với IL2CPP** và lỗi chỉ xuất hiện khi build (không phải trong Editor):

1. **Edit > Project Settings > Player > Other Settings**
2. **Managed Stripping Level**: đổi từ **Medium/High** xuống **Low** hoặc **Minimal**.
3. Build lại.

## Bước 4: Cập nhật Unity / package

- Cập nhật Unity lên bản patch mới nhất của cùng major version (ví dụ 2022.3.x mới nhất).
- Cập nhật các package có build callback (Build Report Tool, CodeStage ACTk, External Dependency Manager, AppLovin MAX) lên bản tương thích với phiên bản Unity bạn dùng.

---

Sau khi xử lý xong, có thể xóa file hướng dẫn này.
