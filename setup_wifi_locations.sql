-- Tạo bảng CompanyWiFiLocations
CREATE TABLE IF NOT EXISTS companywifilocations (
    id SERIAL PRIMARY KEY,
    locationname VARCHAR(200) NOT NULL,
    wifissid VARCHAR(100) NOT NULL,
    wifibssid VARCHAR(50),
    isactive BOOLEAN DEFAULT true,
    createdat TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updatedat TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Thêm WiFi cho phép chấm công
INSERT INTO
    companywifilocations (
        locationname,
        wifissid,
        wifibssid,
        isactive
    )
VALUES (
        'KEYBOX KAFE - Quán Cà Phê',
        'KEYBOX KAFE',
        'b4:5d:50:f7:d0:73',
        true
    );

-- Xem danh sách
SELECT * FROM companywifilocations;

-- Chạy trong database client
UPDATE companywifilocations
SET
    locationname = 'KEYBOX_CN1',
    wifissid = 'KEYBOX KAFE',
    wifibssid = 'b4:5d:50:f7:d0:73'
WHERE
    id = 2;

DELETE FROM companywifilocations WHERE id = 1;

select * from usershifts where userid = 8;

insert into usershifts VALUES ( 151, 8, 3, '02/12/2025', 'Assigned' );