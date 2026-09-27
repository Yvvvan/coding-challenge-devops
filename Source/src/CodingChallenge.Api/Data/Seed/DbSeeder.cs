using CodingChallenge.Api.Domain.Entities;

namespace CodingChallenge.Api.Data.Seed;

/// <summary>Seeds a small, representative dataset for everyday manual testing/demoing.</summary>
public static class DbSeeder
{
    public static void SeedSampleData(AppDbContext context)
    {
        if (context.Orders.Any() || context.Boards.Any() || context.Components.Any())
        {
            return;
        }

        var powerComponent = new Component { Name = "Resistor 10k", Description = "0805 SMD resistor", Quantity = 5000 };
        var capacitor = new Component { Name = "Capacitor 100nF", Description = "0603 SMD ceramic capacitor", Quantity = 8000 };
        var mcu = new Component { Name = "MCU STM32F103", Description = "32-bit ARM Cortex-M3 microcontroller", Quantity = 750 };
        var connector = new Component { Name = "Connector JST-XH 4P", Description = "4-pin wire-to-board connector", Quantity = 1200 };

        var mainBoard = new Board { Name = "Main Control Board", Description = "Primary controller board for the assembly line", Length = 120, Width = 80 };
        var sensorBoard = new Board { Name = "Sensor Interface Board", Description = "Interfaces analog sensors to the main board", Length = 60, Width = 40 };

        mainBoard.BoardComponents.Add(new BoardComponent { Board = mainBoard, Component = mcu });
        mainBoard.BoardComponents.Add(new BoardComponent { Board = mainBoard, Component = powerComponent });
        mainBoard.BoardComponents.Add(new BoardComponent { Board = mainBoard, Component = capacitor });
        sensorBoard.BoardComponents.Add(new BoardComponent { Board = sensorBoard, Component = connector });
        sensorBoard.BoardComponents.Add(new BoardComponent { Board = sensorBoard, Component = capacitor });

        var firstOrder = new Order { Name = "Order 2026-001", Description = "Pilot batch for customer A", OrderDate = DateTime.UtcNow.AddDays(-5), Status = OrderStatus.Active };
        var secondOrder = new Order { Name = "Order 2026-002", Description = "Production batch for customer B", OrderDate = DateTime.UtcNow.AddDays(-1), Status = OrderStatus.Active };

        firstOrder.OrderBoards.Add(new OrderBoard { Order = firstOrder, Board = mainBoard });
        secondOrder.OrderBoards.Add(new OrderBoard { Order = secondOrder, Board = mainBoard });
        secondOrder.OrderBoards.Add(new OrderBoard { Order = secondOrder, Board = sensorBoard });

        context.Components.AddRange(powerComponent, capacitor, mcu, connector);
        context.Boards.AddRange(mainBoard, sensorBoard);
        context.Orders.AddRange(firstOrder, secondOrder);

        context.SaveChanges();
    }
}
