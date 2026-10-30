# Domain: Alerting

Alerting là optional, không thuộc TraceFlow Core. Capability này chỉ nên được cân nhắc sau khi ingestion, processing, search, reliability và operational insights đã ổn định.

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

Nếu triển khai, alerting phải dựa trên search/aggregation đáng tin cậy và phải có cơ chế tránh gửi thông báo lặp quá nhiều trong cùng một incident. Notification production-grade không nằm trong scope core.
