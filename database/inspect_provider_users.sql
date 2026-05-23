SELECT u."Email",
       COALESCE(u."FirstName", '(null)') AS first_name,
       COALESCE(u."LastName", '(null)')  AS last_name,
       p."Tier",
       p.hierarchy
FROM users u
JOIN providers p ON p."UserId" = u."Id"
ORDER BY u."Email";
