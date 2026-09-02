namespace HMS.Shared.Contracts.Inventory;

/// <summary>Diterbitkan saat barang masuk / receiving.</summary>
[EventName(EventNames.InventoryReceived)]
public sealed record InventoryReceivedEvent : IntegrationEvent
{
    public Guid ItemId { get; init; }
    public string ItemCode { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public string? BatchNumber { get; init; }
    public DateOnly? ExpirationDate { get; init; }
}

/// <summary>Diterbitkan saat stok di-adjust manual.</summary>
[EventName(EventNames.InventoryStockAdjusted)]
public sealed record InventoryAdjustedEvent : IntegrationEvent
{
    public Guid ItemId { get; init; }
    public string ItemCode { get; init; } = string.Empty;
    public int Delta { get; init; }
    public int NewBalance { get; init; }
    public string Reason { get; init; } = string.Empty;
}

/// <summary>Diterbitkan saat stok di bawah reorder level.</summary>
[EventName(EventNames.InventoryLowStock)]
public sealed record InventoryLowStockEvent : IntegrationEvent
{
    public Guid ItemId { get; init; }
    public string ItemCode { get; init; } = string.Empty;
    public string ItemName { get; init; } = string.Empty;
    public int CurrentStock { get; init; }
    public int ReorderLevel { get; init; }
}

/// <summary>Diterbitkan saat stok habis.</summary>
[EventName(EventNames.InventoryOutOfStock)]
public sealed record InventoryOutOfStockEvent : IntegrationEvent
{
    public Guid ItemId { get; init; }
    public string ItemCode { get; init; } = string.Empty;
    public string ItemName { get; init; } = string.Empty;
}

/// <summary>Diterbitkan saat batch mendekati / memasuki kadaluwarsa.</summary>
[EventName(EventNames.InventoryExpiring)]
public sealed record InventoryExpiringEvent : IntegrationEvent
{
    public Guid ItemId { get; init; }
    public string ItemCode { get; init; } = string.Empty;
    public string ItemName { get; init; } = string.Empty;
    public string BatchNumber { get; init; } = string.Empty;
    public DateOnly ExpirationDate { get; init; }
    public int RemainingQuantity { get; init; }
}