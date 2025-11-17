# Permission Management API Documentation

## Overview
Complete API endpoints for managing permissions and role-permission relationships.

## Base URLs
- **Permission Management**: `/api/permission`
- **Role-Permission Management**: `/api/role/{roleId}/permissions`

## Authorization
All endpoints require the `ROLE` permission (checked via `[Permission(Const.Permissions.Role)]` attribute).

---

## Permission Management Endpoints

### 1. Get Paginated Permissions
Get a paginated list of permissions with optional search.

**Endpoint**: `GET /api/permission`

**Query Parameters**:
- `pageNumber` (int, optional): Page number (default: 1)
- `pageSize` (int, optional): Items per page (default: 10)
- `search` (string, optional): Search by permission code

**Response Example**:
```json
{
  "statusCode": 200,
  "message": "Data retrieved successfully.",
  "data": {
    "dataList": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "code": "ACCOUNT"
      },
      {
        "id": "4fa85f64-5717-4562-b3fc-2c963f66afa7",
        "code": "PRODUCT"
      }
    ],
    "totalCount": 12,
    "pageIndex": 1,
    "pageSize": 10
  }
}
```

---

### 2. Get All Permissions
Get all permissions without pagination.

**Endpoint**: `GET /api/permission/all`

**Response Example**:
```json
{
  "statusCode": 200,
  "message": "Data retrieved successfully.",
  "data": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "code": "ACCOUNT"
    },
    {
      "id": "4fa85f64-5717-4562-b3fc-2c963f66afa7",
      "code": "PRODUCT"
    }
  ]
}
```

---

### 3. Get Permission By ID
Get a specific permission by its ID.

**Endpoint**: `GET /api/permission/{id}`

**Path Parameters**:
- `id` (guid): Permission ID

**Response Example**:
```json
{
  "statusCode": 200,
  "message": "Data retrieved successfully.",
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "code": "ACCOUNT"
  }
}
```

---

### 4. Create Permission
Create a new permission.

**Endpoint**: `POST /api/permission`

**Request Body**:
```json
{
  "code": "INVENTORY"
}
```

**Response Example**:
```json
{
  "statusCode": 201,
  "message": "Data created successfully.",
  "data": {
    "id": "5fa85f64-5717-4562-b3fc-2c963f66afa8",
    "code": "INVENTORY"
  }
}
```

**Notes**:
- Permission code will be automatically converted to uppercase
- Duplicate codes are not allowed (returns 409 Conflict)

---

### 5. Update Permission
Update an existing permission.

**Endpoint**: `PUT /api/permission`

**Request Body**:
```json
{
  "id": "5fa85f64-5717-4562-b3fc-2c963f66afa8",
  "code": "INVENTORY_MANAGEMENT"
}
```

**Response Example**:
```json
{
  "statusCode": 200,
  "message": "Data updated successfully.",
  "data": {
    "id": "5fa85f64-5717-4562-b3fc-2c963f66afa8",
    "code": "INVENTORY_MANAGEMENT"
  }
}
```

**Notes**:
- Permission code will be automatically converted to uppercase
- Cannot update to a code that already exists (returns 409 Conflict)

---

### 6. Delete Permission
Delete a permission.

**Endpoint**: `DELETE /api/permission/{id}`

**Path Parameters**:
- `id` (guid): Permission ID

**Response Example**:
```json
{
  "statusCode": 200,
  "message": "Data deleted successfully."
}
```

**Notes**:
- Cannot delete permissions that are assigned to roles (returns 409 Conflict)
- Must remove permission from all roles first

---

## Role-Permission Management Endpoints

### 7. Get Role Permissions
Get all permissions assigned to a specific role.

**Endpoint**: `GET /api/role/{roleId}/permissions`

**Path Parameters**:
- `roleId` (guid): Role ID

**Response Example**:
```json
{
  "statusCode": 200,
  "message": "Data retrieved successfully.",
  "data": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "code": "ACCOUNT"
    },
    {
      "id": "4fa85f64-5717-4562-b3fc-2c963f66afa7",
      "code": "PRODUCT"
    }
  ]
}
```

---

### 8. Assign Permissions to Role
Add permissions to a role without removing existing ones.

**Endpoint**: `POST /api/role/{roleId}/permissions/assign`

**Path Parameters**:
- `roleId` (guid): Role ID

**Request Body**:
```json
[
  "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "4fa85f64-5717-4562-b3fc-2c963f66afa7",
  "5fa85f64-5717-4562-b3fc-2c963f66afa8"
]
```

**Response Example**:
```json
{
  "statusCode": 200,
  "message": "Data updated successfully."
}
```

**Notes**:
- Only adds new permissions (doesn't remove existing ones)
- Prevents duplicate assignments automatically
- Validates that all permission IDs exist

---

### 9. Update Role Permissions
Replace all permissions for a role with a new set.

**Endpoint**: `PUT /api/role/{roleId}/permissions`

**Path Parameters**:
- `roleId` (guid): Role ID

**Request Body**:
```json
[
  "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "4fa85f64-5717-4562-b3fc-2c963f66afa7"
]
```

**Response Example**:
```json
{
  "statusCode": 200,
  "message": "Data updated successfully."
}
```

**Notes**:
- Clears ALL existing permissions and replaces with new set
- Pass empty array `[]` to remove all permissions
- Validates that all permission IDs exist

---

### 10. Remove Permission from Role
Remove a specific permission from a role.

**Endpoint**: `DELETE /api/role/{roleId}/permissions/{permissionId}`

**Path Parameters**:
- `roleId` (guid): Role ID
- `permissionId` (guid): Permission ID to remove

**Response Example**:
```json
{
  "statusCode": 200,
  "message": "Data deleted successfully."
}
```

**Notes**:
- Only removes the specified permission
- Other permissions remain unchanged

---

## Updated Role Endpoints

The following existing role endpoints now include permission data in their responses:

### Get Role By ID
**Endpoint**: `GET /api/role/{id}`

**Response Example**:
```json
{
  "statusCode": 200,
  "message": "Data retrieved successfully.",
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "Manager",
    "accountCount": 5,
    "permissions": [
      {
        "id": "4fa85f64-5717-4562-b3fc-2c963f66afa7",
        "code": "PRODUCT"
      },
      {
        "id": "5fa85f64-5717-4562-b3fc-2c963f66afa8",
        "code": "BRANCH"
      }
    ]
  }
}
```

---

### Get Paged Roles
**Endpoint**: `GET /api/role`

All roles now include their permissions in the response.

---

### Get All Roles
**Endpoint**: `GET /api/role/all`

All roles now include their permissions in the response.

---

## Error Responses

### 400 Bad Request
Missing or invalid parameters.

```json
{
  "statusCode": 400,
  "message": "An unexpected error occurred."
}
```

### 404 Not Found
Resource not found.

```json
{
  "statusCode": 404,
  "message": "Failed to retrieve data."
}
```

### 409 Conflict
Duplicate or constraint violation.

```json
{
  "statusCode": 409,
  "message": "Permission code already exists"
}
```

or

```json
{
  "statusCode": 409,
  "message": "Cannot delete permission that is assigned to roles"
}
```

---

## Common Use Cases

### 1. Setup New Role with Permissions

```bash
# Step 1: Create the role
POST /api/role
{
  "name": "Warehouse Manager"
}

# Step 2: Get all available permissions
GET /api/permission/all

# Step 3: Assign permissions to the new role
POST /api/role/{roleId}/permissions/assign
[
  "permission-id-1",
  "permission-id-2",
  "permission-id-3"
]
```

### 2. Update Role Permissions

```bash
# Option A: Replace all permissions at once
PUT /api/role/{roleId}/permissions
[
  "new-permission-id-1",
  "new-permission-id-2"
]

# Option B: Add specific permissions
POST /api/role/{roleId}/permissions/assign
[
  "additional-permission-id"
]

# Option C: Remove specific permission
DELETE /api/role/{roleId}/permissions/{permissionId}
```

### 3. View Role Permissions

```bash
# Get role with all permissions
GET /api/role/{roleId}

# Or get just the permissions
GET /api/role/{roleId}/permissions
```

### 4. Manage Permissions

```bash
# Create new permission
POST /api/permission
{
  "code": "REPORTS"
}

# Search for permissions
GET /api/permission?search=PRODUCT&pageNumber=1&pageSize=10

# Update permission code
PUT /api/permission
{
  "id": "permission-id",
  "code": "ADVANCED_REPORTS"
}

# Delete permission (must not be assigned to any roles)
DELETE /api/permission/{id}
```

---

## Permission Codes in System

Based on `Const.Permissions`, the following permission codes are available:

- `ACCOUNT` - Account management
- `ROLE` - Role management (includes permission management)
- `PRODUCT` - Product management
- `BRANCH` - Branch management
- `BRANCH_STOCK` - Branch stock management
- `BRANCH_DEBT` - Branch debt management
- `SUPPLIER_DEBT` - Supplier debt management
- `WAREHOUSE` - Warehouse management
- `PURCHASE_ORDER` - Purchase order management
- `PURCHASE_RETURN` - Purchase return management
- `TRANSFER_ORDER` - Transfer order management
- `STOCK_ADJUSTMENT` - Stock adjustment management
- `FILE` - File management

---

## Authentication

All endpoints require authentication via JWT token. Include the token in the request header:

```
Authorization: Bearer <your-jwt-token>
```

The authenticated user's ID is automatically extracted from the token for audit purposes.

---

## Notes

1. **Admin Role**: Users with the "Admin" role automatically have all permissions, regardless of explicit permission assignments.

2. **Permission Codes**: All permission codes are stored and compared in uppercase. Input will be automatically normalized.

3. **Active Roles Only**: Permission checks only consider roles where `EndDate` is null or in the future.

4. **Cascade Protection**: Permissions cannot be deleted if they are assigned to any roles. Remove them from all roles first.

5. **Audit Trail**: All create, update, and delete operations accept an `accountId` parameter for audit purposes (automatically extracted from JWT token in controllers).

