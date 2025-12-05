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

insert into
    usershifts (
        userid,
        shiftid,
        shiftdate,
        status
    )
VALUES (
        8,
        3,
        2025 -12 -02,
        "assigned"
    );

INSERT INTO
    usershifts (
        userid,
        shiftid,
        shiftdate,
        status
    )
VALUES (
        8,
        3,
        '2025-12-02',
        'assigned'
    );

select * from departments;

UPDATE Users SET RoleId = 2 WHERE Id = 18;

select * from users;

select * from positiontitles;

ALTER TABLE users ADD COLUMN basesalary DECIMAL(10, 2) DEFAULT 0;

COMMENT ON COLUMN users.basesalary IS 'Lương cứng tháng (cho Manager)';