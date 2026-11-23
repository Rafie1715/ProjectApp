# CutiApp Frontend

Frontend Angular 17 standalone untuk pengajuan dan persetujuan cuti. Halaman mengikuti empat mockup pada Lampiran A.

## Menjalankan aplikasi

Aktifkan PostgreSQL sesuai `ConnectionStrings:DefaultConnection` di konfigurasi backend. Dari folder `Backend`, jalankan API di terminal pertama. Saat mulai, API menerapkan migrasi dan menyiapkan akun awal.

```sh
dotnet run --launch-profile http
```

Dari folder `Frontend`, jalankan Angular di terminal kedua.

```sh
npm ci
npm start
```

Buka `http://localhost:4200`. Untuk membuat build produksi, jalankan `npm run build`. Hasilnya ada di `dist/cuti-app/browser`.

## Menghubungkan backend

Alamat dasar API di `src/environments/environment.ts` adalah `http://localhost:5000/api`, sesuai profil `http` di backend. Kebijakan CORS backend mengizinkan origin `http://localhost:4200`.

Endpoint berikut tersedia di `AuthController` dan `LeaveRequestsController`. Path relatif terhadap alamat dasar API.

| Method | Path | Request atau hasil |
| --- | --- | --- |
| POST | `/auth/login` | Kirim `{ username, password }`, terima `{ token, fullName, role }` |
| GET | `/leave-requests/mine` | Array pengajuan milik pengguna yang login |
| GET | `/leave-requests/balance` | `{ remainingDays: number }` untuk pengguna yang login |
| GET | `/leave-requests` | Array seluruh pengajuan |
| POST | `/leave-requests` | Kirim `{ startDate, endDate, reason }`, terima pengajuan berstatus Pending (201) |
| PUT | `/leave-requests/{id}/approve` | Terima pengajuan berstatus Approved (200) |
| PUT | `/leave-requests/{id}/reject` | Terima pengajuan berstatus Rejected (200) |
| DELETE | `/leave-requests/{id}` | Batalkan pengajuan sendiri yang masih Pending, terima `{ message }` (200) |

Setiap item daftar berisi `id`, `userId`, `fullName`, `startDate`, `endDate`, `reason`, `status`, dan `createdAt`. Status API memakai `Pending`, `Approved`, atau `Rejected`. Tanggal formulir dikirim sebagai string `yyyy-MM-dd`. Aksi tulis menerima respons sukses JSON atau respons kosong. Untuk kegagalan bisnis, kembalikan status HTTP yang sesuai dan pesan pada field `message` atau `detail`.

Semua halaman selain login membutuhkan sesi. Setelah login, role `Manager` diarahkan ke `/approval` dan pengguna lain ke `/home`. Backend memeriksa token, kepemilikan pengajuan, dan saldo cuti.
