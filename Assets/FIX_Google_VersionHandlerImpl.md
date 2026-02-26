# Sửa lỗi: Could not load assembly 'Google.VersionHandlerImpl'

Lỗi xảy ra khi Firebase Editor khởi tạo (`GenerateXmlFromGoogleServicesJson`) và cần **External Dependency Manager for Unity (EDM4U)** – assembly `Google.VersionHandlerImpl` không tìm thấy.

## Nguyên nhân thường gặp

- Project đang dùng EDM từ file **.tgz** (local) nhưng trong **Assets** vẫn còn thư mục **ExternalDependencyManager** và **PlayServicesResolver** (chỉ có tài liệu, không có file DLL). Unity có thể tìm assembly sai chỗ hoặc bản EDM trong package (tgz) thiếu/không tương thích.
- Bản EDM đóng gói (tgz) có thể thiếu hoặc không đúng cấu trúc so với bản Firebase cần.

## Cách 1: Dùng EDM từ Git (khuyến nghị)

Đảm bảo EDM lấy từ nguồn chính thức và đầy đủ:

1. Mở **Packages/manifest.json**.
2. Đổi dòng `com.google.external-dependency-manager` từ file tgz sang Git:

   **Từ:**
   ```json
   "com.google.external-dependency-manager": "file:../Assets/Base/3Party/Third/com.google.external-dependency-manager-1.2.186.tgz"
   ```

   **Thành:**
   ```json
   "com.google.external-dependency-manager": "https://github.com/google-unity/external-dependency-manager.git"
   ```

3. Lưu file, quay lại Unity (sẽ tự resolve package).
4. Nếu vẫn lỗi: **đóng Unity** → xóa thư mục **Library** → mở lại project.

## Cách 2: Giữ EDM từ file .tgz (offline)

Nếu bạn cần dùng bản EDM local (tgz):

1. **Tạm đổi tên** hai thư mục trong Assets để Unity không dùng nhầm bản “rác” (chỉ có README/CHANGELOG, không có DLL):
   - `Assets/ExternalDependencyManager` → `Assets/ExternalDependencyManager_OLD`
   - `Assets/PlayServicesResolver` → `Assets/PlayServicesResolver_OLD`
2. **Đóng Unity** → xóa thư mục **Library** → mở lại project.
3. Nếu vẫn báo thiếu `Google.VersionHandlerImpl`: bản EDM trong file tgz có thể thiếu assembly này → nên chuyển sang **Cách 1** (cài EDM từ Git).

## Cách 3: Cài EDM bằng .unitypackage (chỉ dùng EDM trong Assets)

1. Tải **External Dependency Manager** (.unitypackage) từ [Releases · google-unity/external-dependency-manager](https://github.com/google-unity/external-dependency-manager/releases).
2. Trong **manifest.json**, **xóa** dòng dependency `com.google.external-dependency-manager` (bỏ EDM khỏi Package Manager).
3. Trong Unity: **Assets > Import Package > Custom Package** → chọn file .unitypackage vừa tải → Import.
4. Xóa thư mục **Library**, mở lại project.

---

Sau khi sửa xong, có thể xóa file hướng dẫn này.
