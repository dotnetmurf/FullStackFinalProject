# Code Commenting Instructions for GitHub Copilot

## General Comment Guidelines

### Comment Structure
- Use clear, concise language
- Write in complete sentences
- Start with a capital letter
- End with appropriate punctuation
- Maintain consistent style throughout the codebase

### When to Comment
- Complex algorithms or business logic
- Public API methods and classes
- Non-obvious code sections
- Workarounds or temporary solutions
- Important limitations or constraints
- Dependencies and side effects
- Configuration settings

### When NOT to Comment
- Self-explanatory code
- Obvious implementations
- Redundant information
- Code that's likely to change frequently
- Comments that merely restate the code

## Language-Specific Comment Formats

### C# Comments

#### XML Documentation Comments
```csharp
/// <summary>
/// Brief description of the class/method/property
/// </summary>
/// <param name="parameterName">Description of the parameter</param>
/// <returns>Description of the return value</returns>
/// <exception cref="ExceptionType">When the exception is thrown</exception>
/// <remarks>
/// Additional information about the implementation
/// </remarks>
```

#### Method Comments
```csharp
// Public method comment example
/// <summary>
/// Calculates the total price including tax
/// </summary>
/// <param name="basePrice">The original price before tax</param>
/// <param name="taxRate">The tax rate as a decimal (e.g., 0.08 for 8%)</param>
/// <returns>The total price with tax applied</returns>
public decimal CalculateTotalPrice(decimal basePrice, decimal taxRate)
```

#### Property Comments
```csharp
/// <summary>
/// Gets or sets the customer's full name
/// </summary>
/// <value>The customer's first and last name</value>
public string CustomerName { get; set; }
```

### JavaScript/TypeScript Comments

#### JSDoc Comments
```javascript
/**
 * Function description
 * @param {parameterType} parameterName - Parameter description
 * @returns {returnType} Description of the return value
 * @throws {errorType} Description of when the error is thrown
 */
```

#### Class Comments
```typescript
/**
 * Class description
 * @implements {InterfaceName}
 * @extends {ParentClassName}
 */
```

### Razor/Blazor Comments

#### Component Comments
```csharp
@* 
    Component: ComponentName
    Purpose: Brief description of the component's purpose
    Usage: Example of how to use the component
*@
```

#### Code-behind Comments
```csharp
/// <summary>
/// Represents a component that displays user information
/// </summary>
/// <remarks>
/// This component handles user authentication state and profile display
/// </remarks>
public partial class UserProfileComponent : ComponentBase
```

## Best Practices

### Method Comments Should Include
- Purpose of the method
- Parameters and their requirements
- Return value and its meaning
- Exceptions that might be thrown
- Any side effects
- Usage examples if non-obvious

### Class Comments Should Include
- Purpose of the class
- Responsibilities and collaborators
- Important implementation notes
- Usage examples
- Any limitations or constraints

### Variable and Property Comments
- Only comment if the purpose isn't obvious from the name
- Explain any constraints or valid values
- Document any side effects
- Note any dependencies

### Configuration Comments
- Purpose of each setting
- Valid values and constraints
- Default values
- Impact of changes

### API Documentation
- Complete parameter documentation
- Response formats
- Error conditions
- Rate limits or quotas
- Authentication requirements
- Example requests and responses

## Comment Templates

### Bug Fix Template
```csharp
// Bug Fix: [Bug ID or Description]
// Root Cause: Brief explanation of the issue
// Solution: Description of the fix
// Date: YYYY-MM-DD
```

### TODO Template
```csharp
// TODO: Brief description of what needs to be done
// Priority: [High/Medium/Low]
// Created: YYYY-MM-DD
// Owner: [Developer Name]
```

### Temporary Code Template
```csharp
// TEMPORARY: Description of why this is temporary
// Created: YYYY-MM-DD
// Remove/Replace by: YYYY-MM-DD
// Alternative: Description of the permanent solution
```

## Comment Maintenance

### Regular Review
- Keep comments up to date with code changes
- Remove obsolete comments
- Update incorrect comments
- Expand unclear comments
- Add missing comments for new complexity

### Comment Quality Checklist
- [ ] Comments are accurate and current
- [ ] Grammar and spelling are correct
- [ ] Comments add value beyond the code
- [ ] Complex logic is well explained
- [ ] Public APIs are fully documented
- [ ] Comments follow team standards
- [ ] No redundant or obvious comments
- [ ] Comments explain "why" not just "what"

## Examples of Good Comments

### Business Logic
```csharp
// Calculate pro-rated refund based on company policy:
// - Full refund if within 30 days of purchase
// - 50% refund if between 31-60 days
// - No refund after 60 days
```

### Complex Algorithm
```csharp
// Implementation of the Knuth-Morris-Pratt string matching algorithm
// Time Complexity: O(n + m) where n is text length and m is pattern length
// Space Complexity: O(m) for the partial match table
```

### Configuration
```csharp
// Redis cache settings
// Timeout: How long to wait for cache operations (ms)
// RetryCount: Number of retry attempts for failed operations
// MaxPoolSize: Maximum number of connections in the pool
```

## Examples of Bad Comments (To Avoid)

### Obvious Comments
```csharp
// Bad: Set the name
name = value;

// Good: No comment needed for obvious operations
```

### Redundant Comments
```csharp
// Bad: Calculate the total by adding tax
var total = subtotal + tax;

// Good: Only comment complex calculations or business rules
```

### Outdated Comments
```csharp
// Bad: Check if user is admin (now includes supervisor role)
if (user.IsAdmin || user.IsSupervisor)

// Good: Check if user has administrative privileges
if (user.IsAdmin || user.IsSupervisor)
```

## Special Instructions

### Security-Related Comments
- Mark security-sensitive code clearly
- Document security assumptions
- Note potential vulnerabilities
- Reference relevant security policies
- Document authentication requirements

### Performance-Related Comments
- Document performance characteristics
- Note time/space complexity
- Explain optimization decisions
- Document benchmarks if applicable
- Note scale limitations

### Generated Code Comments
- Mark generated code clearly
- Include generator version
- Note any manual modifications
- Document regeneration process

Remember: Comments should tell the "why" not the "what". The code itself should be clear enough to tell what it's doing, while comments explain why it's doing it that way.