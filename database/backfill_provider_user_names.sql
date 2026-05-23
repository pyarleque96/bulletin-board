-- Backfill FirstName/LastName for provider users seeded without them.
-- Solo actualiza cuando el campo está vacío para no pisar ediciones manuales.

UPDATE users SET
    "FirstName" = COALESCE(NULLIF("FirstName", ''), 'Marco'),
    "LastName"  = COALESCE(NULLIF("LastName",  ''), 'Delgado')
WHERE "Email" = 'vip1@bulletindells.com';

UPDATE users SET
    "FirstName" = COALESCE(NULLIF("FirstName", ''), 'Sandra'),
    "LastName"  = COALESCE(NULLIF("LastName",  ''), 'Reyes')
WHERE "Email" = 'vip2@bulletindells.com';

UPDATE users SET
    "FirstName" = COALESCE(NULLIF("FirstName", ''), 'Jorge'),
    "LastName"  = COALESCE(NULLIF("LastName",  ''), 'Castillo')
WHERE "Email" = 'ver1@bulletindells.com';

UPDATE users SET
    "FirstName" = COALESCE(NULLIF("FirstName", ''), 'Marisol'),
    "LastName"  = COALESCE(NULLIF("LastName",  ''), 'Fuentes')
WHERE "Email" = 'ver2@bulletindells.com';

UPDATE users SET
    "FirstName" = COALESCE(NULLIF("FirstName", ''), 'Alejandro'),
    "LastName"  = COALESCE(NULLIF("LastName",  ''), 'Vega')
WHERE "Email" = 'ver3@bulletindells.com';

UPDATE users SET
    "FirstName" = COALESCE(NULLIF("FirstName", ''), 'Lucía'),
    "LastName"  = COALESCE(NULLIF("LastName",  ''), 'Torres')
WHERE "Email" = 'ver4@bulletindells.com';

-- Fallback genérico: cualquier otro provider sin nombre.
UPDATE users u SET
    "FirstName" = 'Provider',
    "LastName"  = LEFT(u."Id"::text, 8)
FROM providers p
WHERE p."UserId" = u."Id"
  AND (u."FirstName" IS NULL OR u."FirstName" = ''
       OR u."LastName" IS NULL OR u."LastName" = '');
