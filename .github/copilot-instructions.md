# Copilot Instructions

## XML Documentation Comments

- Always use multi-line `<summary>` tags (three lines: opening tag, description, closing tag):
  ```csharp
  /// <summary>
  /// Description text here.
  /// </summary>
  ```
- Never use single-line summaries like `/// <summary>Description</summary>`.

## Brace Placement

- **Switch expressions:** The opening `{` must always be on a new line directly below the expression, at the same indentation level:
  ```csharp
  return expression switch
         {
             Pattern => ...,
         };
  ```
- **Object initializers:** The opening `{` must always be on a new line directly below the `new` keyword, at the same indentation level as `new`:
  ```csharp
  var obj = new MyType
            {
                Property = value
            };
  ```
