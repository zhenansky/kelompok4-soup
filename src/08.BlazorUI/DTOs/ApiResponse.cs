namespace MyApp.BlazorUI.DTOs
{
  /// <summary>
  /// Standard API Response Wrapper
  /// Purpose: Provide consistent success response format
  /// 
  /// Why wrap responses?
  /// 1. Consistent structure across all endpoints
  /// 2. Easy to add metadata (timestamp, pagination, etc)
  /// 3. Clear success/failure indication
  /// 4. Additional context with messages
  /// 
  /// Example success response:
  /// {
  ///   "success": true,
  ///   "data": { "transactionId": "TXN123", ... },
  ///   "message": "Transfer completed successfully",
  ///   "timestamp": "2025-10-05T12:30:45Z"
  /// }
  /// </summary>
  public class ApiResponse<T>
  {
    /// <summary>
    /// Indicates if request was successful
    /// true = Success, false = Error
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Response data (type depends on endpoint)
    /// Examples:
    /// - TransactionResponseDto for transfer endpoint
    /// - List<TransactionResponseDto> for list endpoint
    /// - null for delete endpoint
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Success or informational message
    /// Examples:
    /// - "Transfer completed successfully"
    /// - "Account created"
    /// - "No transactions found"
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// When the response was generated
    /// Useful for caching and debugging
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
  }

  public class ErrorResponse
  {
    public string? ErrorCode { get; set; }
    public string? Message { get; set; }
    public object? Details { get; set; }
    public DateTime Timestamp { get; set; }
    public string? TraceId { get; set; }
  }

  /// <summary>
  /// Non-generic API Response
  /// Purpose: For endpoints that don't return specific data
  /// Example: Delete operations, void methods
  /// </summary>
  public class ApiResponse : ApiResponse<object>
  {
  }
}