using MyApp.BlazorUI.DTOs; // Pastikan namespace DTO benar

namespace MyApp.BlazorUI.Services
{
    // Service untuk mengelola state keranjang
    public class CartService
    {
        // Daftar item di keranjang (disimpan di memori selama aplikasi berjalan)
        private List<CartItem> _items = new List<CartItem>();

        // Event agar komponen lain tahu jika keranjang berubah
        public event Action? OnChange;

        // Mengambil semua item
        public List<CartItem> GetItems() => _items;

        // Menambah item baru
        public void AddItem(CartItem item)
        {
            // Cek apakah item (course + schedule) sudah ada
            var existingItem = _items.FirstOrDefault(i =>
                i.MenuCourseId == item.MenuCourseId && i.ScheduleId == item.ScheduleId);

            if (existingItem == null)
            {
                _items.Add(item);
                NotifyStateChanged(); // Beri tahu komponen lain
            }
            // Jika sudah ada, kita tidak melakukan apa-apa (atau bisa tambah kuantitas nanti)
        }

        // Menghapus item
        public void RemoveItem(CartItem itemToRemove)
        {
            var item = _items.FirstOrDefault(i =>
                i.MenuCourseId == itemToRemove.MenuCourseId && i.ScheduleId == itemToRemove.ScheduleId);

            if (item != null)
            {
                _items.Remove(item);
                NotifyStateChanged(); // Beri tahu komponen lain
            }
        }

        // Menghapus semua item
        public void ClearCart()
        {
            _items.Clear();
            NotifyStateChanged();
        }

        // Helper untuk memicu event OnChange
        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}