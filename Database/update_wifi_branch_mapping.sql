-- =====================================================
-- Update WiFi Locations to map with Branches
-- =====================================================

-- Update existing WiFi location to link with KEYBOX branch
UPDATE companywifilocations
SET
    branch_id = 3 -- KEYBOX KAFE - BRANCH 3
WHERE
    wifissid = 'KEYBOX_CN1';

-- Insert sample WiFi locations for other branches
INSERT INTO
    companywifilocations (
        locationname,
        wifissid,
        wifibssid,
        isactive,
        branch_id,
        createdat,
        updatedat
    )
VALUES
    -- SAIGON CAMPUS - BRANCH 1
    (
        'HUTECH AB - Floor 1',
        'HUTECH_AB_F1',
        'AA:BB:CC:DD:EE:01',
        true,
        1,
        NOW(),
        NOW()
    ),
    (
        'HUTECH AB - Floor 2',
        'HUTECH_AB_F2',
        'AA:BB:CC:DD:EE:02',
        true,
        1,
        NOW(),
        NOW()
    ),
    -- THU DUC CAMPUS - BRANCH 2
    (
        'HUTECH KHUE - Building E1',
        'HUTECH_KHUE_E1',
        'BB:CC:DD:EE:FF:01',
        true,
        2,
        NOW(),
        NOW()
    ),
    (
        'HUTECH KHUE - Building E2',
        'HUTECH_KHUE_E2',
        'BB:CC:DD:EE:FF:02',
        true,
        2,
        NOW(),
        NOW()
    ),
    -- KEYBOX KAFE - BRANCH 3 (already has one, add more if needed)
    (
        'KEYBOX KAFE - Main Area',
        'KEYBOX_MAIN',
        'CC:DD:EE:FF:00:01',
        true,
        3,
        NOW(),
        NOW()
    ),
    -- MYPHONE HOTSPOT - BRANCH 4
    (
        'MYPHONE HOTSPOT - Zone A',
        'MYPHONE_ZONE_A',
        'DD:EE:FF:00:11:01',
        true,
        4,
        NOW(),
        NOW()
    ),
    (
        'MYPHONE HOTSPOT - Zone B',
        'MYPHONE_ZONE_B',
        'DD:EE:FF:00:11:02',
        true,
        4,
        NOW(),
        NOW()
    )
ON CONFLICT DO NOTHING;

-- Verify the updates
SELECT w.id, w.locationname, w.wifissid, w.wifibssid, w.branch_id, b.branch_code, b.branch_name
FROM
    companywifilocations w
    LEFT JOIN branches b ON w.branch_id = b.id
ORDER BY w.branch_id, w.id;