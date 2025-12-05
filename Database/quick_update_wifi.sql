-- Quick update for existing WiFi location
UPDATE companywifilocations
SET
    branch_id = 3 -- KEYBOX KAFE - BRANCH 3
WHERE
    wifissid = 'KEYBOX KAFE';

-- Verify with branch info and BSSID
SELECT w.id, w.locationname, w.wifissid, w.wifibssid, w.isactive, w.branch_id, b.branch_code, b.branch_name, b.location_address
FROM
    companywifilocations w
    LEFT JOIN branches b ON w.branch_id = b.id
ORDER BY w.branch_id, w.id;

SELECT w.id, w.locationname, w.wifissid, w.wifibssid, w.branch_id, b.branch_name
FROM
    companywifilocations w
    LEFT JOIN branches b ON w.branch_id = b.id;

SELECT * FROM companywifilocations WHERE branch_id = 3;