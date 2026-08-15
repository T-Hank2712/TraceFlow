# Domain: Alerting

Alerting cho phép người dùng định nghĩa rule dựa trên log query, threshold và time window.

## Ví dụ rule

```text
service = payment-service
level = ERROR
count > 50
window = 5 minutes
```

## Thành phần dự kiến

- Alert Rule.
- Alert Event.
- Notification Channel.
- Alert Evaluation Worker.

## Kênh thông báo

- Email.
- Telegram.
- Slack.
- Webhook.

## Nguyên tắc

Alerting phải dựa trên search/aggregation đáng tin cậy và phải có cơ chế tránh gửi thông báo lặp quá nhiều trong cùng một incident.
