using System;
using System.Collections.Generic;
using System.Linq;

// ---------- Модели ----------

class Slot
{
    public int Id { get; set; }
    public DateTime StartAt { get; set; }
    public string TrackConfig { get; set; } = "";
    public int FreeKarts { get; set; }
}

class Booking
{
    public int Id { get; set; }
    public int SlotId { get; set; }
    public string Equipment { get; set; } = "";
}

// ---------- "База данных" в памяти ----------

static class Db
{
    public static List<Slot> Slots = new()
    {
        new Slot { Id = 1, StartAt = DateTime.Today.AddHours(12), TrackConfig = "Короткая (новички)", FreeKarts = 8 },
        new Slot { Id = 2, StartAt = DateTime.Today.AddHours(14), TrackConfig = "Длинная (опытные)", FreeKarts = 14 },
        new Slot { Id = 3, StartAt = DateTime.Today.AddHours(18), TrackConfig = "Короткая (новички)", FreeKarts = 0 },
    };

    public static List<Booking> Bookings = new();

    public static int NextBookingId = 1;
}

class Program
{
    static void Main()
    {
        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("=== Картинг-центр: мок API ===");
            Console.WriteLine("1. GET /schedule — заезды на сегодня");
            Console.WriteLine("2. POST /bookings — создать бронь");
            Console.WriteLine("3. DELETE /bookings/{id} — отменить бронь");
            Console.WriteLine("0. Выход");
            Console.Write("Выберите пункт: ");

            string? choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    GetSchedule();
                    break;
                case "2":
                    CreateBooking();
                    break;
                case "3":
                    CancelBooking();
                    break;
                case "0":
                    return;
                default:
                    Console.WriteLine("Неизвестный пункт меню.");
                    break;
            }
        }
    }

    // GET /schedule
    static void GetSchedule()
    {
        Console.WriteLine();
        Console.WriteLine("--- Расписание на сегодня ---");

        if (Db.Slots.Count == 0)
        {
            Console.WriteLine("Пока нет доступных заездов.");
            return;
        }

        foreach (var slot in Db.Slots)
        {
            Console.WriteLine(
                $"ID {slot.Id} | {slot.StartAt:HH:mm} | {slot.TrackConfig} | Свободно карт: {slot.FreeKarts}");
        }
    }

    // POST /bookings
    static void CreateBooking()
    {
        Console.WriteLine();
        Console.Write("Введите ID заезда: ");
        if (!int.TryParse(Console.ReadLine(), out int slotId))
        {
            Console.WriteLine("Некорректный ID.");
            return;
        }

        var slot = Db.Slots.FirstOrDefault(s => s.Id == slotId);
        if (slot == null)
        {
            Console.WriteLine("Заезд с таким ID не найден.");
            return;
        }

        if (slot.FreeKarts <= 0)
        {
            Console.WriteLine("Свободных мест на этот заезд нет.");
            return;
        }

        Console.Write("Тип экипировки (own/rental): ");
        string? equipment = Console.ReadLine();
        if (equipment != "own" && equipment != "rental")
        {
            Console.WriteLine("Некорректный тип экипировки. Ожидалось: own или rental.");
            return;
        }

        var booking = new Booking
        {
            Id = Db.NextBookingId++,
            SlotId = slot.Id,
            Equipment = equipment
        };

        slot.FreeKarts--;
        Db.Bookings.Add(booking);

        Console.WriteLine($"Бронь создана. ID брони: {booking.Id}. Свободных мест теперь: {slot.FreeKarts}.");
    }

    // DELETE /bookings/{id}
    static void CancelBooking()
    {
        Console.WriteLine();
        Console.Write("Введите ID брони для отмены: ");
        if (!int.TryParse(Console.ReadLine(), out int bookingId))
        {
            Console.WriteLine("Некорректный ID.");
            return;
        }

        var booking = Db.Bookings.FirstOrDefault(b => b.Id == bookingId);
        if (booking == null)
        {
            Console.WriteLine("Бронь с таким ID не найдена.");
            return;
        }

        var slot = Db.Slots.FirstOrDefault(s => s.Id == booking.SlotId);
        if (slot != null)
        {
            slot.FreeKarts++;
        }

        Db.Bookings.Remove(booking);

        Console.WriteLine($"Бронь {bookingId} отменена. Место возвращено.");
    }
}
