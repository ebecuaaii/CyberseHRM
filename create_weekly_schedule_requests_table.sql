-- Tạo bảng cho yêu cầu đăng ký lịch làm việc theo tuần
CREATE TABLE IF NOT EXISTS weeklyschedule_requests (
    id SERIAL PRIMARY KEY,
    userid INTEGER NOT NULL REFERENCES users (id),
    week_start_date DATE NOT NULL,
    week_end_date DATE NOT NULL,
    status VARCHAR(20) DEFAULT 'pending', -- pending, reviewed, scheduled
    availability_data JSONB NOT NULL, -- Lưu thông tin ca có thể làm theo ngày
    note TEXT,
    reviewed_by INTEGER REFERENCES users (id),
    reviewed_at TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT unique_user_week UNIQUE (userid, week_start_date)
);

-- Index để tăng tốc query
CREATE INDEX IF NOT EXISTS idx_weeklyschedule_userid ON weeklyschedule_requests (userid);

CREATE INDEX IF NOT EXISTS idx_weeklyschedule_status ON weeklyschedule_requests (status);

CREATE INDEX IF NOT EXISTS idx_weeklyschedule_week ON weeklyschedule_requests (week_start_date);

-- Comment
COMMENT ON TABLE weeklyschedule_requests IS 'Yêu cầu đăng ký lịch làm việc theo tuần của nhân viên';

COMMENT ON COLUMN weeklyschedule_requests.availability_data IS 'JSON chứa thông tin ca có thể làm theo từng ngày trong tuần';

-- Example availability_data format:
-- {
--   "monday": [1, 2],    -- Có thể làm shift 1 hoặc 2
--   "tuesday": [1, 3],
--   "wednesday": [2],
--   "thursday": [1, 2, 3],
--   "friday": [1],
--   "saturday": [],      -- Không thể làm
--   "sunday": []
-- }