
# Online Order Rule Engine with n8n  

## 🚀 Overview  
This project is an **online ordering system** that uses **.NET Core Web API** and **SQL Server**, with **n8n** acting as the **Rule Engine**. The Rule Engine evaluates customer and product eligibility based on their scores, approving or rejecting orders based on whether they meet the criteria.

## 🔧 Architecture  
1. **.NET Core Web API**  
   - Manages incoming order requests and provides endpoints to fetch customer and product data.
2. **SQL Server**  
   - Stores customer and product scores.
3. **n8n Rule Engine**  
   - Validates whether an order should be approved or rejected based on the scores retrieved from the API.

## ⚙️ How It Works  
1. A customer places an order via the **.NET Core API**.  
2. The order is sent to **n8n** for rule evaluation.  
3. **n8n** fetches:  
   - **Customer Score** from the API endpoint:  
     ```http
     GET /users/{id}
     ```  
   - **Product Score** from the API endpoint:  
     ```http
     GET /product/{id}
     ```  
4. **n8n** compares both scores:  
   - **If the customer score is greater than or equal to the product score threshold → Order Approved**  
   - **If the scores don’t match → Order Rejected**  
5. The decision is sent back to the **.NET API**, which updates the order status in **SQL Server**.

---

## 🔄 n8n Workflow Implementation  

1. **Trigger the Workflow**:  
   - A **Webhook Node** receives the order details from the .NET API.
2. **Fetch Customer & Product Scores**:  
   - Use **HTTP Request Nodes** to call `/users/{id}` to retrieve `customer_score`.  
   - Use **HTTP Request Nodes** to call `/product/{id}` to retrieve `product_score_threshold`.
3. **Decision Making**:  
   - The **IF Node** compares `customer_score` with `product_score_threshold`.  
   - If the customer score meets the threshold, the order is approved; if not, it is rejected.
4. **Respond to Webhook**:  
   - **If approved** → Update the order status to "Confirmed" in the **.NET API**.  
   - **If rejected** → Update the order status to "Rejected" in the **.NET API**.  

---

## ⚖️ Example Rule Logic  

```python
IF customer_score >= product_score_threshold THEN
    Approve order
ELSE
    Reject order
```

---

## 🛠️ Technology Stack  
- **Backend:** .NET Core Web API  
- **Database:** SQL Server  
- **Rule Engine:** n8n  
- **Workflow Automation:** n8n Nodes (Webhook, HTTP Request, IF Condition)  

---

## 📝 Setup Instructions

#### **1. Docker Configuration for n8n**  
- The Docker configurations for n8n are located in the `docker` folder.
- Set up necessary environment variables and volume mappings in the configuration files.
- Use the following command to start the setup:
  ```bash
  docker-compose up -d
  ```
  This will bring up n8n with all the required configurations.

#### **2. Setup .NET Core API**  
- Clone the repository and configure the **SQL Server connection string**.  
- Run database migrations to create tables for customers, products, and orders.  
- Make sure the following endpoints are set up in the API:
  - `GET /users/{id}` → Returns customer score.  
  - `GET /product/{id}` → Returns product score threshold.  
  - `PATCH /orders/{id}` → Updates the order status.

#### **3. Deploy n8n**  
- Use Docker or install n8n locally:
  ```bash
  docker run -it --rm     -p 5678:5678     -v ~/.n8n:/home/node/.n8n     n8nio/n8n
  ```
- Create a **Webhook Trigger** in n8n to listen for order requests.  
- Use **HTTP Request Nodes** to fetch customer and product scores.  
- Implement the workflow to compare scores and respond back to the **.NET API**.

#### **4. Test the Integration**  
- Place an order via the **.NET Core API**.  
- Verify the order status in **SQL Server**.  
- Monitor n8n executions to confirm that the rule engine is functioning as expected.

---

## 🔧 WorkFlow Example for n8n  

Here's the **JSON** file for the n8n workflow. Copy and paste it into n8n to run it:

```json
{
  "name": "Order Rule Engine Workflow",
  "nodes": [
    {
      "parameters": {
        "httpMethod": "POST",
        "path": "RuleEngine",
        "responseMode": "responseNode",
        "options": {}
      },
      "type": "n8n-nodes-base.webhook",
      "typeVersion": 2,
      "position": [-180, 180],
      "id": "77be25d3-2fea-417a-9933-b0ab495142cb",
      "name": "Webhook",
      "webhookId": "cab59b31-26f8-493a-b68f-a08fcd5db279"
    },
    {
      "parameters": {
        "url": "=http://192.168.242.11:5000/products/{{$json.body.productId}}",
        "options": {}
      },
      "type": "n8n-nodes-base.httpRequest",
      "typeVersion": 4.2,
      "position": [60, -100],
      "id": "e8aa5627-1442-47b6-a9dd-57f9530ed339",
      "name": "Get Product"
    },
    {
      "parameters": {
        "url": "=http://192.168.242.11:5000/users/{{$('Webhook').item.json.body.userId}}",
        "options": {}
      },
      "type": "n8n-nodes-base.httpRequest",
      "typeVersion": 4.2,
      "position": [280, 180],
      "id": "2ab2f920-b880-4530-ba5c-591fc5783014",
      "name": "Get User"
    },
    {
      "parameters": {
        "respondWith": "json",
        "responseBody": "{
  "Response": "You Do Not Have Enough Bonus To Buy This Product"
}",
        "options": {}
      },
      "type": "n8n-nodes-base.respondToWebhook",
      "typeVersion": 1.1,
      "position": [760, -20],
      "id": "3ae0c3ab-ecff-4a4e-b274-1c221d8ac511",
      "name": "Respond to Webhook"
    },
    {
      "parameters": {
        "respondWith": "json",
        "responseBody": "{
  "Response": "Order Confirmed"
}",
        "options": {}
      },
      "type": "n8n-nodes-base.respondToWebhook",
      "typeVersion": 1.1,
      "position": [760, 280],
      "id": "8d4e0db8-f583-4b6f-a407-0d039796ee8b",
      "name": "Respond to Webhook1"
    },
    {
      "parameters": {
        "conditions": {
          "options": {
            "caseSensitive": true,
            "leftValue": "",
            "typeValidation": "strict",
            "version": 2
          },
          "conditions": [
            {
              "id": "5f3dcb94-147e-487d-b8c6-c4189be03541",
              "leftValue": "={{ $('Get Product').item.json.limitBonus }}",
              "rightValue": "={{ $json.bonus }}",
              "operator": {
                "type": "number",
                "operation": "gt"
              }
            }
          ],
          "combinator": "and"
        },
        "options": {}
      },
      "type": "n8n-nodes-base.if",
      "typeVersion": 2.2,
      "position": [520, 180],
      "id": "651b2eba-6c8e-4475-b013-8dc8520839a9",
      "name": "Check For Bonus"
    }
  ],
  "pinData": {},
  "connections": {
    "Webhook": {
      "main": [
        [
          {
            "node": "Get Product",
            "type": "main",
            "index": 0
          }
        ]
      ]
    },
    "Get Product": {
      "main": [
        [
          {
            "node": "Get User",
            "type": "main",
            "index": 0
          }
        ]
      ]
    },
    "Get User": {
      "main": [
        [
          {
            "node": "Check For Bonus",
            "type": "main",
            "index": 0
          }
        ]
      ]
    },
    "Check For Bonus": {
      "main": [
        [
          {
            "node": "Respond to Webhook",
            "type": "main",
            "index": 0
          }
        ],
        [
          {
            "node": "Respond to Webhook1",
            "type": "main",
            "index": 0
          }
        ]
      ]
    }
  },
  "active": true,
  "settings": {
    "executionOrder": "v1"
  },
  "versionId": "24a16f1a-6b64-44f1-beae-3e61914eca5a",
  "meta": {
    "instanceId": "fa5e312e1050cfb971f8686fa55892a63a467a26bf4aab8dd9a797fe181ab15b"
  },
  "id": "OkNfSrWs9XD2Mtyv",
  "tags": []
}
```

---

## 🚀 Future Enhancements  
- **Implement Caching**: Reduce API calls for improved performance.  
- **Dynamic Pricing & Fraud Detection**: Add more complex rules to improve the decision-making process.  
- **Real-Time Order Processing**: Integrate a messaging system like **RabbitMQ** or **Kafka** for real-time updates.
